module.exports = {
  "/api": {
    target: process.env.DOCKER ? "http://backend:5045" : "http://localhost:5045",
    secure: false,
    changeOrigin: true,
  },
};
