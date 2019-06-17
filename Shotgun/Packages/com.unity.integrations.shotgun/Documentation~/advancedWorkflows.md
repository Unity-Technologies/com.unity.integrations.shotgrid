# Advanced Workflows
The tk-config-unity configuration comes with customizations that can greatly
improve artist productivity. 

See [Enabling Advanced Workflows](#enabling-advanced-workflows) for details on how to configure your Shotgun
site to take advantage of these workflows.

## Metadata
When you publish your recordings to Shotgun, Unity embeds the current project path 
and the scene asset path in the created Version entity `sg_unity_metadata` field:

<img src="images/metadata.png" style="border: 1px solid black"/>

This allows Shotgun to start in the right context (right project, 
right scene) when launched from a Version or Note entity.

### Launching from a Version entity
You can launch Unity from a Version entity in the Versions page:

<img src="images/launch_from_version_1.png" style="border: 1px solid black"/>

Or from a Version entity page

<img src="images/launch_from_version_2.png" style="border: 1px solid black"/>

### Launching from a Note entity
You can launch Unity from a Note entity in the Notes page:

<img src="images/launch_from_note_1.png" style="border: 1px solid black"/>

Or from a Note entity page

<img src="images/launch_from_note_2.png" style="border: 1px solid black"/>

In all cases, Unity should launch directly without presenting the Unity Hub or 
the Project Selection dialog.

**Note:** Launching from a Version entity or from a Note entity straight to the 
right Unity project and scene only works if the user has their 
project in the location that is saved in the `sg_unity_metadata` field.
If there is no matching Unity project, then the Unity Hub/Project Selector will
be launched instead.

## Enabling Advanced Workflows
Unity uses a custom Version entity field named `sg_unity_metadata` in order to
save metadata that is used in advanced workflows. Your Shotgun site administrator
needs to add this custom field:

1. As an administrator, go to the Versions page
2. Select `Manage Version Fields...`  

    <img src="images/manage_version_fields.png" style="border: 1px solid black"/>

3. Create a `text` field, named `Unity Metadata`  

    <img src="images/new_field.png" style="border: 1px solid black"/>

    You should see the new field  

    <img src="images/new_field_2.png" style="border: 1px solid black"/>

4. Go into the Fields page  

    <img src="images/fields.png" style="border: 1px solid black"/>  

    There should be a new field on Version entities. The field name should be `Unity Metadata`, the field code should be `sg_unity_metadata`, the data type should be `text`  

    <img src="images/validate_field.png" style="border: 1px solid black"/>