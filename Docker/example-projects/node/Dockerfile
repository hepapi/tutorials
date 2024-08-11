# Base image
FROM node:14

# Create app directory
WORKDIR /usr/src/app

# Install app dependencies
COPY package.json ./
RUN npm install

# Bundle app source
COPY . .

# Create data directory
RUN mkdir -p /usr/src/app/data

# Expose port and start application
EXPOSE 3000
CMD ["node", "app.js"]
