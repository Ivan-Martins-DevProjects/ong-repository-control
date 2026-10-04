package domain

import (
	"time"
	"uuid"

	"gorm.io/gorm"
)

type StockItem struct {
	ID          uuid.UUID `json:"id" gorm:"type:text;primaryKey`
	Name        string    `json:"name" gorm:"not null,unique"`
	Description string    `json:"description"`
	Active      bool      `json:"active" gorm:"not null"`
	CreatedAt   time.Time `json:"created_at"`
	UpdatedAt   time.Time `json:"updated_at"`
}

func (s *StockItem) BeforeCreate(tx *gorm.DB) (err error) {
	if s.ID == uuid.Nil() {
		s.ID = uuid.New()
	}

	return
}
