# Setting Up tk-config-default2

1. Edit env/includes/engine_locations.yml
2. Add the following section:
    ```
    # Unity
    engines.tk-unity.location:
      type: dev
      path: '{CONFIG_FOLDER}/tk-unity'
    ```

    This assumes that the tk-unity engine is located in a directory named tk-unity at the root of the configuration. Other descriptors will also work, as [documented online](https://developer.shotgunsoftware.com/tk-core/descriptor.html).

3. Edit env/project.yml

    Add this to the includes section:
	```
        - ./includes/settings/tk-unity.yml
	```
        
  Also add the tk-unity engine under engines:
    
    ```
    engines:
      tk-unity: "@settings.tk-unity.all"
    ```

4. Create file tk-unity.yml under env/includes/settings, and add the following to the file:
    ```
    ################################################################################

    includes:
    - ../app_locations.yml
    - ../engine_locations.yml
    - ./tk-multi-breakdown.yml
    - ./tk-multi-loader2.yml
    - ./tk-multi-publish2.yml

    ################################################################################

    # project
    settings.tk-unity.all:
      apps:
        tk-multi-about:
          location: "@apps.tk-multi-about.location"
        
        tk-multi-breakdown: "@settings.tk-multi-breakdown.unity"

        tk-multi-publish2: "@settings.tk-multi-publish2.unity"

        tk-multi-loader2: "@settings.tk-multi-loader2.unity"

        tk-multi-shotgunpanel: "@apps.tk-multi-shotgunpanel.location"

        tk-multi-pythonconsole:
          location: "@apps.tk-multi-pythonconsole.location"

      location: "@engines.tk-unity.location"
      menu_favorites: []
      launch_builtin_plugins: [basic]
      automatic_context_switch: false
    ```

5. In env/includes/settings/tk-multi-breakdown.yml, add the following:
    ```
    settings.tk-multi-breakdown.unity:
      hook_scene_operations: '{engine}/tk-multi-breakdown/tk-unity_scene_operations.py'
      location: "@apps.tk-multi-breakdown.location"
    ```

6. Add the following to env/includes/settings/tk-multi-publish2.yml:
    ```
    settings.tk-multi-publish2.unity:
      help_url: https://support.shotgunsoftware.com/hc/en-us/articles/115000068574-Integrations-User-Guide#The%20Publisher
      collector: "{self}/collector.py:{engine}/tk-multi-publish2/basic/collector.py"
      publish_plugins:
      - name: Publish to Shotgun
        hook: "{self}/publish_file.py"
        settings: {}
      - name: Upload for review
        hook: "{self}/upload_version.py:{engine}/tk-multi-publish2/basic/upload_movie.py"
        settings: {}
      location: "@apps.tk-multi-publish2.location"
    ```

7. Add the following to env/includes/tk-multi-loader2.yml:
    ```
    settings.tk-multi-loader2.unity:
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
      location: "@apps.tk-multi-loader2.location"
    ```
