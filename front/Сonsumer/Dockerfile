FROM node:20-alpine AS build
WORKDIR /app

ARG VITE_API_URL=
ARG VITE_ENABLE_CHAT_HUB=true

ENV VITE_API_URL=$VITE_API_URL
ENV VITE_ENABLE_CHAT_HUB=$VITE_ENABLE_CHAT_HUB

COPY package*.json ./
RUN npm ci

COPY . .
RUN npm run build-only

FROM nginx:stable-alpine AS runtime

COPY docker/nginx/nginx.conf /etc/nginx/nginx.conf
COPY docker/nginx/frontend.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist /usr/share/nginx/html

EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
