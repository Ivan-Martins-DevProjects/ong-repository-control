package repository

import (
	"errors"
	"strings"

	"gorm.io/gorm"

	"github.com/Ivan-Martins-DevProjects/RepoControl/internal/domain"
	apperror "github.com/Ivan-Martins-DevProjects/RepoControl/internal/domain/app_error"
)

type UserRepository interface {
	FindByEmail(email string) (*domain.User, error)
	CreateUser(user domain.User) (string, error)
}

type userRepository struct {
	db *gorm.DB
}

func NewUserRepository(db *gorm.DB) UserRepository {
	return &userRepository{db: db}
}

func (r *userRepository) FindByEmail(email string) (*domain.User, error) {
	var user domain.User

	result := r.db.Where("email = ?", email).First(&user)
	if result.Error != nil {
		if errors.Is(result.Error, gorm.ErrRecordNotFound) {
			return nil, apperror.Unauthorized("Credenciais inválidas", nil)
		}

		return nil, apperror.InternalServerError("Erro ao localizar usuário no banco de dados", result.Error)
	}

	return &user, nil
}

func (r *userRepository) CreateUser(user domain.User) (string, error) {
	if user.Name == "" {
		return "", apperror.BadRequest("Nome inválido", nil)
	}

	result := r.db.Create(&user)
	if result.Error != nil {
		errMsg := result.Error.Error()

		if strings.Contains(errMsg, "UNIQUE constraint failed") {
			if strings.Contains(errMsg, "users.email") || strings.Contains(errMsg, "email") {
				return "", apperror.BadRequest("Email já está cadastrado", result.Error)
			}

			return "", apperror.BadRequest("Violação de chave única", result.Error)
		}

		return "", apperror.InternalServerError("Erro ao criar usuário", result.Error)
	}

	return user.ID.String(), nil
}

func (r *userRepository) DeleteUser(ID uint) error {
	var user domain.User

	result := r.db.First(&user, ID)
	if result.Error != nil {
		if errors.Is(result.Error, gorm.ErrRecordNotFound) {
			return apperror.BadRequest("Usuário não encontrado", result.Error)
		}

		return apperror.InternalServerError("Erro ao localizar usuário", result.Error)
	}

	delResult := r.db.Delete(&user)
	if delResult.Error != nil {
		return apperror.InternalServerError("Erro ao deletar usuário", delResult.Error)
	}

	return nil
}
