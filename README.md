# simplepack
This repository contains the code to establish the comunication between the SimplePack Sigfox sensor and Cloud services.

### json_body_aws.json
Contains the body of the message used in the sigfox callback, either for AWS or Microsoft Azure.

### json_dynamoDB.json
Contains the body of the JSON file configuration for the DynamoDB action in AWS IoT rule.

### HttpTrigger_sigfox_atlas_debug.cs
Contains the code for the Azure function
