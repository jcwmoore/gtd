#!/usr/bin/env sh
set -x
scp package.tgz $REMOTE_USER@$REMOTE_HOST:/var/www/gtd && \
ssh $REMOTE_USER@$REMOTE_HOST 'bash -s' < ./scripts/untar.sh