package database

import (
	"log"
	"os"

	"gorm.io/driver/postgres"
	"gorm.io/gorm"

	"github.com/Ivan-Martins-DevProjects/RepoControl/internal/domain"
)

const (
	UNIQUE_CONSTRAINT = "23505"
)

var DB *gorm.DB

func ConnectDB(useDefault bool) (*gorm.DB, error) {
	dsn := os.Getenv("DATABASE_URL")
	if dsn == "" && useDefault {
		log.Print("String de conexão ao banco de dados não encontrada, usando string padrão.")
		dsn = "postgres://repositorycontrol:repositorycontrol@localhost:5432/repositorycontrol?sslmode=disable"
	}

	if dsn == "" && !useDefault {
		dsn = "postgres://repositorycontrol:repositorycontrol@localhost:5432/repositorycontrol?sslmode=disable"
	}

	var err error
	DB, err = gorm.Open(postgres.Open(dsn), &gorm.Config{})
	if err != nil {
		log.Fatalf("Erro ao conectar ao banco de dados: %v", err)
	}

	err = DB.AutoMigrate(&domain.User{})
	if err != nil {
		log.Fatal("Falha ao migrar tabelas: ", err)
	}

	return DB, nil
}
