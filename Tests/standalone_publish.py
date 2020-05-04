import sgtk
import sg_client

import os
def log(msg):
    with open("C:\\Users\\Baptiste Guidoux\\AppData\\Local\\Unity\\Editor\\standalone_pub.log", 'a') as f:
        f.write(msg + os.linesep)

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
new_version_entities = sg.find("Version", [], ['code'])

# OK
added_version_entities = []
for item in new_version_entities:
    if item not in original_version_entities:
        added_version_entities.append(item)

# name 'original_version_entities' is not defined
try:
    added_version_entities = [item for item in new_version_entities if item not in original_version_entities]
except Exception as e:
    log(str(e))

# name 'original_version_entities' is not defined
try:
    added_version_entities = list(filter(lambda item: item not in original_version_entities, new_version_entities))
except Exception as e:
    log(str(e))

# look for a new version entity with the right name
for entity in added_version_entities:
    if entity['code'] == session_name + ".mp4":
        print('The movie file was successfully published')
        success = True
        break

if success:
    # There is no easy way to communicate results back to Unity. Let's create 
    # a cylinder in the current scene
    UnityEngine = sg_client.GetUnityEngine()
    go = UnityEngine.GameObject.CreatePrimitive(UnityEngine.PrimitiveType.Cylinder)
    go.name = session_name

        