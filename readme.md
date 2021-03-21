# Thoth API Gateway

If you have any questions, send me a message at [Linkedin](https://www.linkedin.com/in/kevinzunigaaguero/)

### Why the name

"Thoth, the "excellent of understanding", observed and wrote down everything that happened and reported it to Ra every morning. As the record keeper of the gods he was paired with the librarian Seshat. Thoth and Seshat knew the future as well as the past. They inscribed a person's fate on the bricks on which their mother gave birth and the length of a king's reign on the leaves of the ished tree (210)."

Source [Ancient](https://www.ancient.eu/Thoth/)

### What is API Gateway

An API gateway provides a single, unified API entry point across one or more internal APIs.

![alt text](https://www.tibco.com/sites/tibco/files/media_entity/2020-05/api-gateway-diagram.svg)

Source [tibco.com](https://www.tibco.com/reference-center/what-is-an-api-gateway), [Microservices](https://microservices.io/patterns/apigateway.html)

## Explain

It is a functional implementation of API Gateway, it contains the basic functionalities such as the in-memory cache per request.

### The origin

I made this project because I had some problems with other third party implementations, basically with the migration of another project from version 2.2 of .NET Core to 3.1 when the communication between the API Gateway that I was using at the time failed due to an SSL error that I did not have before because for previous versions of 3.1 the Kestrel Server did not necessarily require using SSL for connections. This error turned out to be part of the way the HTTP client is initialized, and in this project I solved it.

### Future plans

I have several goals to continue implementing improvements to this API Gateway, the following are:

- Implement connection between HTTP and GRPC to perform the conversion of an HTTP request to Protobuff.
- Throttling limit this in order to prevent the API from being overloaded by too many requests.

These would be the first objectives to achieve.

# Usage

## First steps

In order to start using it, you must first have the following inputs:

- Generate a routes file, in this file you must place the expected URI and the URL where the request must be made, as well as the HTTP methods that are accepted. It is important to follow the file structure. [See file](https://github.com/kevter/Thoth/blob/main/Configuration/routes.json)

```bash
"endpoint": "/api/values" //URI where the request is made
"destination": {
        "path": "https://localhost:5003/api/values", // URL to where the request should be made
        "requiresAuthentication": "false", // whether or not the authorization must be verified
        "httpMethod": [ // methods accepted
 "cache": {
        "TimeSpan": 200 // cache lifetime for each request
  "authenticationService": {
    "path": "http://localhost:8080/api/auth/" // URL of the service to verify authorization

```

- Generate SSL for local use, use the ones that come in the repository and if it is for a productive environment use an SSL from an external provider. It is important that the format is .pfx

## Installation

Two options:

- Pull on your local machine this code.
- Pull docker image, go to [Docker hub](https://hub.docker.com/repository/docker/kevter/thoth)

## Usage

docker-compose.yml add:

```bash
apigateway:
        image: kevter/thoth:latest
        environment:
            - ASPNETCORE_ENVIRONMENT=Development
            - ASPNETCORE_URLS=https://+:443;http://+:80
            - ASPNETCORE_Kestrel__Certificates__Default__Password=Thot_api_gateway_2021
            - ASPNETCORE_Kestrel__Certificates__Default__Path=/Certificate/localhostThoth.pfx
            - Configuration__Routes=/Configuration/routes.json
            - Configuration__Insecure=true // For local host or communicate in microservices
        volumes:
            - ./Certificate/localhostThoth.pfx:/Certificate/localhostThoth.pfx:ro
            - ./Configuration/Configuration/routes.json:/Configuration/routes.json:ro
        ports:
            - '5000:80'
            - '5001:443'
```

For use:

```bash
docker-compose up
```

## Contributing

Pull requests are welcome. For major changes, please open an issue first to discuss what you would like to change.

## License

[MIT](https://github.com/kevter/Thoth/blob/main/LICENSE)
