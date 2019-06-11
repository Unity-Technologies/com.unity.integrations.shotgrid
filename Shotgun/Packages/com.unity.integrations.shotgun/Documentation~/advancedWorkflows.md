# Advanced Workflows
The tk-config-unity configuration comes with customizations that can greatly
improve artist productivity. 

See [Enabling Advanced Workflows](#enabling-custom-workflows) for details on how to configure your Shotgun
site to take advantage of these workflows.

## Metadata
When you publish your recordings to Shotgun, Unity embeds the current project path 
and the scene asset path in the created Version entity `sg_unity_metadata` field:

![metadata](images/metadata.png)

This allows Shotgun to start in the right context (right project, 
right scene) when launched from a Version or Note entity.

### Launching from a Version entity
You can launch Unity from a Version entity in the Versions page:

![launch_from_version_1](images/launch_from_version_1.png)

Or from a Version entity page

![launch_from_version_2](images/launch_from_version_2.png)

### Launching from a Note entity
You can launch Unity from a Note entity in the Notes page:

![launch_from_note_1](images/launch_from_note_1.png)

Or from a Note entity page

![launch_from_note_2](images/launch_from_note_2.png)

In all cases, Unity should launch directly without presenting the Unity Hub or 
the Project Selection dialog.

**Note:** Launching from a Version entity or from a Note entity only works if the 
user launching Unity has its project located in the location that is saved in
the `sg_unity_metadata` field.

## Enabling Advanced Workflows
Unity uses a custom Version entity field named `sg_unity_metadata` in order to
save metadata that is used in advanced workflows. Your Shotgun site administrator
needs to add this custom field:

1. As an administrator, go to the Versions page
2. Select `Manage Version Fields...`  
![manage_version_fields](images/manage_version_fields.png)  
3. Create a `text` field, named `Unity Metadata`  
![new_field](images/new_field.png)  
You should see the new field  
![new_field_2](images/new_field_2.png)  
4. Go into the Fields page  
![fields](images/fields.png)  
There should be a new field on Version entities. The field name should be `Unity Metadata`, the field code should be `sg_unity_metadata`, the data type should be `text`  
![validate_field](images/validate_field.png)  