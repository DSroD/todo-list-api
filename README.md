Awesome (*and unnecessarily complex*) to-do list api using Microsoft Orleans with MongoDB support for persistence and .Net 5.0 with Swagger to expose ToDo-list API.

To connect to your MongoDB atlas, create **appsettings.json** in Silo\ folder using following template

```json
{
    "databaseName": "[your database name]",
    "atlasConnectionString" : "[your connection string]"
}
```

Client runs SwaggerUI.
Grains in Silo represent notes in to-do list.