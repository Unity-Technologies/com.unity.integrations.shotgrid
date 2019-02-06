import os
import json
import UnityEditor

def parseMetadata():
    # get meta data from the environment variable
    unity_metadata_json = os.environ.get('SHOTGUN_UNITY_METADATA')
    if not unity_metadata_json:
        return
    
    unity_metadata = None
    try:
        unity_metadata = json.loads(unity_metadata_json)
    except Exception, e:
        self.logger.warning('Exception while parsing the sg_unity_metadata field: {}. The JSON data is probably invalid and will be ignored'.format(e))
        return
        
    if not unity_metadata or not unity_metadata.has_key('scenePath'):
        return
    
    # open the correct scene in Unity
    UnityEditor.SceneManagement.EditorSceneManager.OpenScene(unity_metadata["scenePath"])
    
    # now that we have set the scene, remove the environment variable
    os.environ.pop("SHOTGUN_UNITY_METADATA")

parseMetadata()