FROM node:current-alpine AS spa
EXPOSE 8080
USER node

WORKDIR /app

RUN mkdir /home/node/.npm-global
ENV PATH=/home/node/.npm-global/bin:$PATH
ENV NPM_CONFIG_PREFIX=/home/node/.npm-global

RUN npm install -g @vue/cli

ENTRYPOINT npm run serve