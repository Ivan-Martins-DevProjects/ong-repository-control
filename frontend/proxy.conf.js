const target = process.env.DOCKER === "true"
  ? "http://backend-go:8080"
  : "http://localhost:8080";

module.exports = {
  "/api": {
    target,
    secure: false,
    changeOrigin: true,
  },
};
