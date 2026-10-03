package token

import "time"

const (
	HS256 = "HS256"
	ECDSA = "ECDSA"
)

var ExpireIn8 = time.Now().Add(time.Hour * 8).Unix()
