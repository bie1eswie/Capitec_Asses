FROM node:22-alpine AS build
WORKDIR /workspace

COPY package.json package-lock.json ./
RUN npm ci
COPY . .
RUN npm run build:customer

FROM nginxinc/nginx-unprivileged:1.27-alpine
COPY nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /workspace/dist/customer/browser/ /usr/share/nginx/html/
EXPOSE 8080