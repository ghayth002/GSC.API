pipeline {
    agent any

    stages {
        stage('Build') {
            steps {
              
                sh 'mvn clean compile'
            }
        }

        stage(' Tests') {
            steps {
                sh 'mvn test'
            }
        }

        stage('SonarQube Analysis') {
            steps {
                withSonarQubeEnv('SonarQube') {
                   
                    sh 'mvn sonar:sonar'
                }
            }
        }

     
    }
   
}