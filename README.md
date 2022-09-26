# PrisonSystemPresentation
### This repository represents the presentation tier of a larger 3-tier distributed system. The other tiers can be found in the following repos:
  * Application Tier: https://github.com/Mishanull/PrisonSystemApplication
  * Data Access Tier: https://github.com/Mishanull/PrisonSystemDataAccess
 
    The system is a three-tier architecture that consists of presentation, application, and 
data access layers. The presentation layer is developed in  C# 
and uses the Blazor framework to create an interactive UI to handle all the 
interaction with the user. The application layer written in Java acts as a server and 
contains most of the business logic. Communication between the two is carried out using 
the Advanced Message Queuing Protocol (AMQP) supported by a RabbitMQ message 
broker. Messages are sent to dedicated queues to which the server listens and then 
sends replies with requested data. 
  The data access layer is also developed in the C# 
language and is connected to an SQLite database using Entity Framework Core (EFC), 
where all the system data is stored. Communication between it and the application tier is facilitated using 
HTTP (REST) protocol. Data access has exposed web API endpoints in form of URLs to which 
the application layer can make HTTP requests (POST, GET, PUT, PATCH, DELETE) in 
order to manipulate the data and perform CRUD operations.
  This project is a system representing a prison facility. It contains multiple types of 
users with different permissions and access - warden, guard and a visitor. Warden is the 
administrator who adds and manages accounts and work shift schedules of the guards, 
sends alerts and validates requests from the visitors. The warden is also in charge of 
creating and managing prisoners, who are not represented as actors in this system since
they have no interaction with it. Guards have access to functionalities required by their 
tasks - they have an overview of the prisoners and can update their information to a 
limited extent. Visitors are people outside the prison, who do not have an account and 
therefore their only interaction with the system is requesting a visit at a specified time 
and date with one prisoner. 
  The project is carried out using agile development practices, where a product backlog 
containing all user stories and requirements is created during analysis, and the main tool used for Scrum is Jira, in this case.

## The instructions for running this are as follows:

* Clone all of the 3 repos in separate projects
* Modify the data source URL in the code inside the data access tier to correspond to your local path (Unfortunately we couldn't host a database for free indefinetely anywhere)
* Run all 3 programs in whatever order you want
* Once you start, use the following credentials for logging in as an admin: username: mike, password: 12345 
* Follow the user guide in this repo to understand how to use the system, if it is not intuitive for you.
