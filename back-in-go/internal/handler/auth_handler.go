package handler

import (
	"net/http"

	"github.com/gin-gonic/gin"

	dto "github.com/Ivan-Martins-DevProjects/RepoControl/internal/domain/DTO"
	apperror "github.com/Ivan-Martins-DevProjects/RepoControl/internal/domain/app_error"
	"github.com/Ivan-Martins-DevProjects/RepoControl/internal/service"
)

type AuthHandler struct {
	authService *service.AuthService
}

func NewAuthHandler(authService *service.AuthService) *AuthHandler {
	return &AuthHandler{
		authService: authService,
	}
}

type LoginRequest struct {
	Email    string `json:"email" binding:"required,email"`
	Password string `json:"password" binding:"required"`
}

func (h *AuthHandler) Login(c *gin.Context) {
	var req LoginRequest

	if err := c.ShouldBindJSON(&req); err != nil {
		c.JSON(http.StatusBadRequest, gin.H{
			"error": err.Error(),
		})
		return
	}

	token, err := h.authService.Login(req.Email, req.Password)
	if err != nil {
		c.JSON(http.StatusUnauthorized, gin.H{
			"error": err.Error(),
		})
		return
	}

	c.SetCookie(
		"access_token",
		token.GetValue(),
		3600*8,
		"/",
		"localhost",
		false,
		true,
	)

	c.Status(http.StatusAccepted)
}

func (h *AuthHandler) CreateUser(c *gin.Context) {
	var request dto.CreateUserRequest

	if err := c.ShouldBindJSON(&request); err != nil {
		c.Error(apperror.BadRequest(
			"Dados incorretos",
			err,
		))
		return
	}

	token, err := h.authService.CreateUser(request)
	if err != nil {
		c.Error(err)
		return
	}

	c.SetCookie(
		"access_token",
		token.GetValue(),
		3600*8,
		"/",
		"localhost",
		false,
		true,
	)
	c.Status(http.StatusCreated)
}
