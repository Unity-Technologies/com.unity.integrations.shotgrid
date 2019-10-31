#
# Test that the version does or does not include -preview.N at the end.
#

import argparse
import json
import os
import re
import sys

verbose = False

def get_version():
    import __main__
    d = os.path.dirname(__main__.__file__)
    package_json = os.path.join(d, "..", "package.json")
    if verbose: 
        print("check_version: loading '{}'".format(package_json))
    with open(package_json) as f:
        package = json.load(f)
    if verbose:
        print("check_version: found version '{}'".format(package['version']))
    return package['version']

def main(args):
    version = get_version()
    if args.require_dev_version:
        regex = r'^[0-9]+\.[0-9]+\.[0-9]+-preview\.[0-9]+$'
        okmsg = 'is a dev version number'
        errmsg = 'does not match a dev version number like x.y.z-preview.n'
    else:
        regex = r'^[0-9]+\.[0-9]+\.[0-9]+(-preview)?$'
        okmsg = 'is a production version number'
        errmsg = 'does not match a publish version number like x.y.z or x.y.z-preview'
    m = re.match(regex, version)
    if m:
        if verbose: print("'{}' {}".format(version, okmsg))
        return True
    else:
        if verbose: print("'{}' {}".format(version, errmsg))
        return False

if __name__ == '__main__':
    parser = argparse.ArgumentParser(description='Check the version in package.json matches expectations. By default, checks it\'s a production version.')
    parser.add_argument('-d', '--is-dev', action='store_true', dest='require_dev_version', help='Fail unless we have a dev version (x.y.z-preview.N)')
    parser.add_argument('-v', '--verbose', action='store_true', dest='verbose', help='Print debug information')
    args = parser.parse_args()
    verbose = args.verbose

    sys.exit( 0 if main(args) else 1 )
