package main

import (
	"net/http"
	"os"
	"time"

	"github.com/gin-gonic/gin"
	"golang.org/x/time/rate"

	"github.com/Ivan-Martins-DevProjects/RepoControl/internal/database"
	"github.com/Ivan-Martins-DevProjects/RepoControl/internal/handler"
	"github.com/Ivan-Martins-DevProjects/RepoControl/internal/handler/middleware"
	"github.com/Ivan-Martins-DevProjects/RepoControl/internal/repository"
	"github.com/Ivan-Martins-DevProjects/RepoControl/internal/service"
)

type Product struct {
	ID    string  `json:"id"`
	Name  string  `json:"name" binding:"required"`
	Price float64 `json:"price" binding:"required,gt=0"`
}

var products = []Product{
	{ID: "1", Name: "Notebook", Price: 450.00},
	{ID: "2", Name: "Mouse", Price: 150.00},
}

func main() {
	router := gin.Default()
	//Permitir em média 3 requisições por minuto, com pico (burst) de até 5 requisições instantâneas por IP
	limiter := middleware.NewRateLimiter(rate.Every(time.Minute/3), 5)
	router.Use(limiter.Middleware(), middleware.ErrorHandler())

	db, _ := database.ConnectDB(false)

	// Auth Resources
	userRepository := repository.NewUserRepository(db)
	authService := service.NewAuthService(
		userRepository,
		os.Getenv("HASH_SECRET"),
	)
	authHandler := handler.NewAuthHandler(authService)

	v1 := router.Group("/api")
	v1.Use(middleware.ErrorHandler())
	{
		authorization := v1.Group("/auth")
		authorization.Use(limiter.Middleware())
		{
			authorization.POST("/login", authHandler.Login)
			authorization.POST("/register", authHandler.CreateUser)
		}
	}

	router.Run(":8080")
}

func getProducts(c *gin.Context) {
	c.JSON(http.StatusOK, products)
}
