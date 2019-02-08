import os
import json
import UnityEditor
import UnityEngine

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
    
    if unity_metadata.has_key('frameNumber'):
        UnityEditor.EditorApplication.ExecuteMenuItem("Window/Sequencing/Timeline")
        
        playableDirectors = UnityEngine.Object.FindObjectsOfType[UnityEngine.Playables.PlayableDirector]()
        if playableDirectors and len(playableDirectors) > 0:
            frameNumber = int(unity_metadata["frameNumber"])
            try:
                playableDirector = playableDirectors[0] # just take the first one
                
                timeline = playableDirector.playableAsset
                
                fps = timeline.editorSettings.fps
                playableDirector.time = frameNumber / fps
                
                # focus on the PlayableDirector in the Timeline window
                UnityEditor.Selection.activeObject = playableDirector;
            except Exception, e:
                UnityEngine.Debug.LogWarning("Unable to focus on Timeline: " + str(e))
            
    
    # now that we have set the scene, remove the environment variable
    os.environ.pop("SHOTGUN_UNITY_METADATA")

parseMetadata()