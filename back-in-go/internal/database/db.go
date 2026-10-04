package database

import (
	"log"

	"gorm.io/driver/sqlite"
	"gorm.io/gorm"

	"github.com/Ivan-Martins-DevProjects/RepoControl/internal/domain"
)

func ConnectDB() (*gorm.DB, error) {
	db, err := gorm.Open(sqlite.Open("data.db"), &gorm.Config{})
	if err != nil {
		log.Fatal("Erro ao abir banco de dados local: ", err)
	}

	err = db.AutoMigrate(&domain.User{})
	if err != nil {
		log.Fatal("Falha ao migrar tabela user: ", err)
	}

	err = db.AutoMigrate(&domain.StockItem{})
	if err != nil {
		log.Fatal("Falha ao migrar tabela stock_item: ", err)
	}

	return db, nil
}
