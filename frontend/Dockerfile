FROM node:22-alpine AS build
WORKDIR /workspace

COPY package.json package-lock.json ./
RUN npm ci
COPY . .
RUN npm run build:customer && npm run build:admin

FROM nginxinc/nginx-unprivileged:1.27-alpine AS customer
COPY nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /workspace/dist/customer/browser/ /usr/share/nginx/html/
EXPOSE 8080

FROM nginxinc/nginx-unprivileged:1.27-alpine AS admin
COPY nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /workspace/dist/admin/browser/ /usr/share/nginx/html/
EXPOSE 8080