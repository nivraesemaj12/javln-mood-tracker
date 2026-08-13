
FROM node:18 AS build

WORKDIR /app

# Install the dependencies
COPY package*.json ./
RUN npm install

# Copy all app files.
COPY . .

RUN npm run build --prod

# Use nginx to serve the web client.
FROM nginx:alpine

# Copy the client files to the default public location for nginx
COPY --from=build /app/dist/web-client /usr/share/nginx/html

FROM nginx:alpine

# Copy the client files to the default public location for nginx
COPY --from=build /app/dist/web-client /usr/share/nginx/html

# Use our custom nginx config so Angular's client-side routing works correctly
# (without this, refreshing or directly visiting a route like /admin returns 404,
# since nginx would otherwise look for a literal file/folder by that name)
COPY nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 80

# Nginx will serve the app on port 80 by default.
EXPOSE 80
