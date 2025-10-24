CREATE DATABASE OficinaDB
GO

USE OficinaDB
GO

CREATE TABLE Pessoa
(
	idPessoa		INT				NOT NULL	PRIMARY KEY	IDENTITY,
	nome			VARCHAR(50)		NOT NULL,
	telefone		VARCHAR(11)		NOT NULL,
	endereco		VARCHAR(300)	NULL,
	documento		VARCHAR(14)		NOT NULL,
	tipo_doc		CHAR			NOT NULL												CHECK	(tipo_doc in ('F', 'J'))
)
GO

CREATE TABLE Funcionario
(
	idFuncionario	INT				NOT NULL	PRIMARY KEY	REFERENCES	Pessoa(idPessoa),
	permissao		INT				NOT NULL	DEFAULT		3								CHECK	(permissao	in	(1, 2, 3)), -- Administrador, escrita ou leitura
	usuario			VARCHAR(16)		NOT NULL	UNIQUE,
	senha			VARCHAR(15)		NOT NULL,
	status			INT				NOT NULL	DEFAULT		1								CHECK	(status		in	(1, 2)) -- Ativo ou inativo
)
GO

CREATE TABLE Cliente
(
	idCliente		INT				NOT NULL	PRIMARY KEY	REFERENCES	Pessoa(idPessoa)
)
GO

CREATE TABLE Orcamento
(
	idOrcamento		INT				NOT NULL	PRIMARY KEY	IDENTITY,
	idFuncionario	INT				NOT NULL	REFERENCES	Funcionario(idFuncionario),
	idCliente		INT				NOT NULL	REFERENCES	Cliente(idCliente),
	data_criacao	DATETIME		NOT NULL,
	data_entrega	DATETIME		NULL,
	status			INT				NOT NULL	DEFAULT		1								CHECK	(status		in	(1, 2, 3)), -- Em aberto, pago ou cancelado
	total			DECIMAL(10, 2)	NOT NULL	CHECK		(total >= 0),
	forma_pgto		VARCHAR(20)		NULL,
	parcelas		INT				NULL
)
GO

CREATE TABLE Veiculo
(
	idVeiculo		INT				NOT NULL	PRIMARY KEY IDENTITY,
	marca			VARCHAR(50)		NOT NULL,
	placa			VARCHAR(7)		NOT NULL	UNIQUE,
	modelo			VARCHAR(50)		NOT NULL
)
GO

CREATE TABLE Servico
(
	idServico		INT				NOT NULL	PRIMARY KEY	IDENTITY,
	descricao		VARCHAR(50)		NULL,
	preco_base		DECIMAL(10, 2)	NULL
)
GO

CREATE TABLE Itens
(
	idItem			INT				NOT NULL	PRIMARY KEY	IDENTITY,
	idOrcamento		INT				NOT NULL	REFERENCES	Orcamento(idOrcamento),
	idVeiculo		INT				NOT NULL	REFERENCES	Veiculo(idVeiculo),
	idServico		INT				NOT NULL	REFERENCES	Servico(idServico),
	data_entrega	DATETIME		NOT NULL,
	qtd				INT				NOT NULL	DEFAULT		1								CHECK	(qtd > 0),
	preco			DECIMAL(10, 2)	NOT NULL,
	descricao		VARCHAR(50)		NULL,
	taxa			DECIMAL(10, 2)	NULL,
	desconto		DECIMAL(10, 2)	NULL,
)
GO


CREATE TABLE Usuario
(	
	UsuarioId	int primary key		identity,
	Nome varchar(100) not null,
	Email varchar(100) not null unique,
	Senha varchar(max) not null
)

INSERT INTO Usuario VALUES ('Fulano', 'fulano@email.com', '123')
INSERT INTO Usuario VALUES ('Ciclano', 'ciclano@email.com', '123')

SELECT * FROM Usuario
