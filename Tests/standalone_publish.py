import sgtk
import sg_client
from typing import List,  Dict


def is_version_published(sg: 'shotgun_api3.shotgun.Shotgun', session_name: str, orig_entities: List[Dict]):
    """ Query Shotgun for all Version entities, compare them to the Verions in `orig_entities` to check if a new version has been created, named with `session_name`
    """ 
    new_entities = sg.find("Version", [], ['code'])
    added_entities = [item for item in new_entities if item not in orig_entities]
    for entity in added_entities:
        if entity['code'] == session_name + ".mp4":
            print('The movie file was successfully published')
            return True
      
    return False

success = False

# need to have an engine running in a context where the publisher has been
# configured.
engine = sgtk.platform.current_engine()
assert engine, 'There is no Shotgun engine'

# get the publish app instance from the engine's list of configured apps
publish_app = engine.apps.get("tk-multi-publish2")

# ensure we have the publisher instance.
if not publish_app:
    raise Exception("The publisher is not configured for this context.")

# create a new publish manager instance
manager = publish_app.create_publish_manager()

# now we can run the collector that is configured for this context
session = manager.collect_session()

# Remember the name of the session item
session_name = None
for item in session:
    if item.type_spec == 'unity.session':
        session_name = item.name

# validate the items to publish
tasks_failed_validation = manager.validate()
assert len(tasks_failed_validation) == 0, 'Validation failed'

# Keep a list of all version entities prior to publishing
sg = engine.shotgun
original_version_entities = sg.find("Version",[], ['code'])

# all good. let's publish and finalize
print('Publishing %s to Shotgun'%(session_name+".mp4"))
try:
    ret_val = manager.publish()
    # If a plugin needed to version up a file name after publish
    # it would be done in the finalize.
    manager.finalize()
except Exception as error:
    print("There was trouble trying to publish!")
    print("Error: %s", error)

# Were version entities added?
success = is_version_published(sg, session_name, original_version_entities)

if success:
    # There is no easy way to communicate results back to Unity. Let's create 
    # a cylinder in the current scene
    UnityEngine = sg_client.GetUnityEngine()
    go = UnityEngine.GameObject.CreatePrimitive(UnityEngine.PrimitiveType.Cylinder)
    go.name = session_name

        