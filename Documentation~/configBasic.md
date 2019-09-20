# Setting Up tk-config-basic

1. Edit env/includes/common/engines.yml
2. Add the following section:
```
# Unity
common.engines.tk-unity.location:
  type: dev
  path: '{CONFIG_FOLDER}/tk-unity'
```
    
This assumes that the tk-unity engine is located in a directory named tk-unity at the root of the configuration. Other descriptors will also work, as [documented online](https://developer.shotgunsoftware.com/tk-core/descriptor.html).

3. Edit env/project.yml


Add this to the includes section:
    * includes/common/apps.yml
    * includes/common/engines.yml

    
Add the tk-unity engine:
```
  tk-unity: 
    apps:
      tk-multi-about: '@common.apps.tk-multi-about'
      tk-multi-publish2: 
        help_url: https://support.shotgunsoftware.com/hc/en-us/articles/115000068574-Integrations-User-Guide#The%20Publisher
        collector: "{self}/collector.py:{engine}/tk-multi-publish2/basic/collector.py"
        publish_plugins:
        - name: Publish to Shotgun
          hook: "{self}/publish_file.py"
          settings: {}
        - name: Upload for review
          hook: "{self}/upload_version.py:{engine}/tk-multi-publish2/basic/upload_movie.py"
          settings: {}
        location: "@common.apps.tk-multi-publish2.location"
      tk-multi-loader2: 
        action_mappings:
          Motion Builder FBX: [import]
        actions_hook: '{engine}/tk-multi-loader2/basic/tk-unity_actions.py'
        entities:
        - caption: Current Project
          type: Hierarchy
          root: "{context.project}"
          publish_filters: []
        - caption: My Tasks
          type: Query
          entity_type: Task
          filters:
          - [project, is, '{context.project}']
          - [task_assignees, is, '{context.user}']
          hierarchy: [entity, content]
        # ignore publishes without a status. with zero config, it is very easy
        # to publish the same path multiple times. the default zero config publish
        # plugins will clear the status of previous publishes of the same path.
        # this filter means only the latest publish will be displayed.
        publish_filters: [["sg_status_list", "is_not", null]]
        location: "@common.apps.tk-multi-loader2.location"
      tk-multi-shotgunpanel:
        location: '@common.apps.tk-multi-shotgunpanel.location'
      tk-multi-pythonconsole: '@common.apps.tk-multi-pythonconsole'
    location: "@common.engines.tk-unity.location"
    menu_favourites: []
    launch_builtin_plugins: [basic]
    automatic_context_switch: false
```