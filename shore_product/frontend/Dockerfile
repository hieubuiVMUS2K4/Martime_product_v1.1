FROM node:20-alpine AS build
WORKDIR /app
COPY package.json ./
RUN npm install --silent
COPY . .
ARG VITE_API_BASE_URL=/api
ENV VITE_API_BASE_URL=${VITE_API_BASE_URL}
RUN npm run build

FROM nginx:stable-alpine
RUN rm /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist /usr/share/nginx/html
COPY nginx/default.conf.template /etc/nginx/templates/default.conf.template
COPY docker-entrypoint.sh /docker-entrypoint.d/40-generate-default-conf.sh
RUN sed -i 's/\r$//' /docker-entrypoint.d/40-generate-default-conf.sh && \
    chmod +x /docker-entrypoint.d/40-generate-default-conf.sh
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]