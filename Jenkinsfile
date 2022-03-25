@Library('global-shared-library') _

def message  = ""

pipeline {
  agent any

  environment {
    ImageName = "harbor.rpgp.cc/tgs/file-service"
    BuildConfiguration = 'Debug'
  }

  stages {
    stage('Prepare') {
      steps {
        sh "echo 'Branch: $GIT_BRANCH'";

        script {
          switch (env.GIT_BRANCH) {
          case ['origin/main', 'main']:
            env.kube_ns = "prod";
            env.k8s_config = 'stag-k8s-config';
            break;
          case ['origin/release', 'release']:
            env.kube_ns = "staging";
            env.k8s_config = 'stag-k8s-config';
            break;
          default:
            env.kube_ns = "dev";
            env.k8s_config = 'k8s-kube-config'
            break;
          }
        }

        configFileProvider([
          configFile(
            fileId: 'tgs.' + env.kube_ns,
            variable: 'tgsconfig')
        ]) {
          script {
            def envItems = readProperties file: "$tgsconfig";

            envItems.each {
              key,
              value -> env."tgs_${key}" = value;
            }

            env.TAG = env.kube_ns + '-' + sh(script: "echo `date +%Y%-m%d-%H%M`", returnStdout: true).trim()
            message += "\nBranch: ${env.GIT_BRANCH}" 
            message += "\nTag: ${TAG}"
            message += "\nEnv: $env.kube_ns"
          }
        }

        sh "echo tag: $TAG"
      }
    }
    stage('Build') {
      steps {
        sh 'dotnet build -c $BuildConfiguration'
      }
    }

    stage('Test') {
      steps {
        sh "dotnet test -c $BuildConfiguration --filter Unit /p:CollectCoverage=true --logger trx --no-build"
      }
    }

    stage('Push packages') {
      steps {
        sh 'dotnet pack -c $BuildConfiguration "FileService.Client/FileService.Client.csproj" --no-build'
        sh 'dotnet nuget push "FileService.Client/bin/$BuildConfiguration/*.nupkg" --source "raypro" --skip-duplicate'
      }
    }

    stage('Building file-service image') {
      steps {
        sh 'dotnet build "FileService/FileService.csproj" -c $BuildConfiguration -t:ContainerBuild -p:DockerfileTag="$ImageName:${TAG}" --no-restore '
      }
    }

    stage('Push Docker Image') {
			steps {
                sh 'docker push $ImageName:${TAG}'
			}
		}

    stage('Deploy') {
      when {
        expression {
          return env.kube_ns == 'prod';
        }
      }

      steps {
        script {
          input message: 'Ready to go?', ok: 'yes';
        }
        
        sh "echo start deploy"
      }
    }

    stage('Deploy file-service') {
      steps {
        script {
          withKubeConfig([credentialsId: env.k8s_config]) {

            def resources = getResources(env.k8s_config, env.kube_ns, "file-service")
            println resources

            sh """
              cat << EOF | yq e -P | helm template deployment/k8s/file-service \
                --set image.tag=${TAG} \
                --set namespace=${kube_ns} \
                --set connectionStrings.file="$tgs_mongo_file" \
                --set nats.urls="$tgs_nats" \
                --set aws.accessKeyId="$tgs_aws_access_key" \
                --set aws.secretAccessKey="$tgs_aws_access_secret" \
                --set aws.region="$tgs_aws_region" \
                --set aws.public.externalUrl="$tgs_file_public_external" \
                --set aws.public.serviceUrl="$tgs_file_public_service" \
                --set aws.public.bucket="$tgs_file_public_bucket" \
                --set aws.private.externalUrl="$tgs_file_private_external" \
                --set aws.private.serviceUrl="$tgs_file_private_service" \
                --set aws.private.bucket="$tgs_file_private_bucket" \
                --set mountMongoCert=$tgs_mountMongoCert \
                -f -
                {
                    "resources": $resources
                }
            """
            sh """ 
              cat << EOF | yq e -P | helm template deployment/k8s/file-service \
                --set image.tag=${TAG} \
                --set namespace=${kube_ns} \
                --set connectionStrings.file="$tgs_mongo_file" \
                --set nats.urls="$tgs_nats" \
                --set aws.accessKeyId="$tgs_aws_access_key" \
                --set aws.secretAccessKey="$tgs_aws_access_secret" \
                --set aws.region="$tgs_aws_region" \
                --set aws.public.externalUrl="$tgs_file_public_external" \
                --set aws.public.serviceUrl="$tgs_file_public_service" \
                --set aws.public.bucket="$tgs_file_public_bucket" \
                --set aws.private.externalUrl="$tgs_file_private_external" \
                --set aws.private.serviceUrl="$tgs_file_private_service" \
                --set aws.private.bucket="$tgs_file_private_bucket" \
                --set mountMongoCert=$tgs_mountMongoCert \
                -f - \
              | kubectl -n ${kube_ns} apply -f -
                  {
                      "resources": $resources
                  }

            """
          }
        }
      }
    }
  }

  post {
    success {
      sendTelegram("Pipeline: ${currentBuild.fullDisplayName} OK!\nDeployed Components: ${message } \n${currentBuild.getBuildCauses()[0].shortDescription}\n")
    }
    failure {
      sendTelegram("<b>Failed Pipeline:</b> ${currentBuild.fullDisplayName} \n Deploy Components: ${message } \n Something is wrong with ${env.BUILD_URL}/console\n${currentBuild.getBuildCauses()[0].shortDescription}\n")
    }
    always {
      step([$class: 'MSTestPublisher', testResultsFile: "**/*.trx", failOnError: false, keepLongStdio: true])
    }
  }
}

def checkIsRunStage(currentName, selectedNames) {
  def arr = selectedNames.split(",")
  def isRun = false

  if (arr.length > 0)
    for (i = 0; i < arr.length; i++) {
      isRun = isRun || (arr[i] == currentName)
    }

  return isRun
}

def sendTelegram(message) {
  def encodedMessage = URLEncoder.encode(message, "UTF-8")

  withCredentials([string(credentialsId: "telegramToken", variable: "TOKEN"), string(credentialsId: "telegramChatId", variable: "CHAT_ID")]) {

    response = httpRequest(consoleLogResponseBody: true,
      contentType: 'APPLICATION_JSON',
      httpMode: 'GET',
      url: "https://api.telegram.org/bot${TOKEN}/sendMessage?text=${encodedMessage}&chat_id=${CHAT_ID}",
      validResponseCodes: '200')
    return response
  }
}

def sendTelegramHTML(message) {
  def encodedMessage = URLEncoder.encode(message, "UTF-8")

  withCredentials([string(credentialsId: "telegramToken", variable: "TOKEN"), string(credentialsId: "telegramChatId", variable: "CHAT_ID")]) {

    response = httpRequest(consoleLogResponseBody: true,
      contentType: 'APPLICATION_JSON',
      httpMode: 'POST',
      requestBody: "parse_mode=HTML&text=${encodedMessage}&chat_id=${CHAT_ID}",
      url: "https://api.telegram.org/bot${TOKEN}/sendMessage",
      validResponseCodes: '200')
    return response
  }
}
