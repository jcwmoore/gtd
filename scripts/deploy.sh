#!/usr/bin/env sh
set -x
scp -o StrictHostKeyChecking=no package.tgz $REMOTE_USER@$REMOTE_HOST:/var/www/gtd  && \
ssh $REMOTE_USER@$REMOTE_HOST -o StrictHostKeyChecking=no 'bash -s' < ./scripts/untar.sh