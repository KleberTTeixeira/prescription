# Angular (prescription-web) - build context: root of the solution folder
#   docker build -f web/frontend/Dockerfile -t prescription-web .
# The Web API address comes from the environment variable API_URL (see proxy.conf.mjs).

FROM node:24-bookworm-slim

# No analytics prompt and no update check from the Angular CLI / npm.
ENV NG_CLI_ANALYTICS=false \
    npm_config_update_notifier=false

WORKDIR /app
# Non-root user provided by the Node images; ng serve writes its cache in /app/.angular.
RUN chown node:node /app
USER node

# Package files first: the install layer is reused while dependencies do not change.
COPY --chown=node:node web/frontend/package.json web/frontend/package-lock.json ./
RUN npm ci

COPY --chown=node:node web/frontend/ ./

EXPOSE 4200
CMD ["./node_modules/.bin/ng", "serve", "--host", "0.0.0.0", "--port", "4200"]
