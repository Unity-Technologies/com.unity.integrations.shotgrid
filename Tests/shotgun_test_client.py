import os
import unity_python.common.scheduling as scheduling
import sys
import time

def log(msg):
    print(msg)

sg_bootstrap_dir = os.path.dirname(os.environ['SHOTGUN_UNITY_BOOTSTRAP_LOCATION'])

if sg_bootstrap_dir not in sys.path:
    sys.path.append(sg_bootstrap_dir)
import sg_bootstrap

_publish_complete = False

class ShotgunTestBootstrapService(sg_bootstrap.ShotgunBootStrapService):
    def exposed_bootstrap_name(self):
        return 'com.unity.integrations.shotgun'

    @scheduling.exec_on_main_thread
    def exposed_standalone_publish(self):
        global _publish_complete
        standalone_publish_script = os.path.join(os.path.dirname(__file__), 'standalone_publish.py')
        exec(open(standalone_publish_script).read())
        _publish_complete = True

    def exposed_bootstrapped(self):
        return sg_bootstrap._shotgun_is_initialized

    def exposed_published(self):
        return _publish_complete

if __name__ == '__main__':
    sg_bootstrap.main(ShotgunTestBootStrapService)
