USE AfReparosAutomotivos
GO

-- Pessoas
INSERT INTO Pessoa		(nome, telefone, endereco, documento, tipo_doc)
VALUES	('Ana Júlia Fernandes',			'17999998888',	'Rua Sobradinho',			'99988877766', 'F'),	-- Funcionária
		('Heloísa Sabino',				'17999987777',	'Avenida 123',				'11122233344', 'F'),	-- Cliente
		('Caio de Paula',				'17123456789',	'Rua da Felicidade',		'22211133344', 'F'),	-- Cliente
		('Enrico Correia',				'17929219292',	'Avenida Marcelo Oliveira',	'33311122233', 'F')		-- Apenas pessoa
GO

INSERT INTO Pessoa		(nome, telefone, endereco, documento, tipo_doc)
VALUES	('Otávio da Costa',					'12345678910',	'Casa',						'44411122233', 'F'),		-- Funcionário
		('Heloísa Sabino de Carvalho',		'10987654321',	'Casa 2',					'10101010101', 'F'),		-- Funcionária
		('Enrico Vilela Correia',			'78787878782',	'Casa 3',					'67676767676', 'F'),		 -- Funcionário
		('Guilherme Furlanetti Verones',	'34343434343',	'Casa 4',					'71823128974', 'F')          -- Funcionário

-- Funcionária
INSERT INTO Funcionario (idFuncionario, permissao, usuario, senha, status)
VALUES	(1,								1,				'anaju',					'19p23$_+12',	1)		-- Admin e ativa
		
INSERT INTO Funcionario (idFuncionario, permissao, usuario, senha, status)
VALUES	(5,								1,				'otavio',					'123',			1),		-- Admin e ativo
		(6,								1,				'helo',						'123',			1),		-- Admin e ativa
		(7,								1,				'enrico',					'123',			1),		-- Admin e ativo
		(8,								1,				'caio',						'123',			1)		-- Admin e ativo

-- Clientes
INSERT INTO Cliente
VALUES	(2), -- Heloísa
		(3)  -- Caio

-- Veículos
INSERT INTO Veiculo		(marca, placa, modelo)
VALUES	('Ford',						'ABC1234',		'Fiesta'),
		('Volkswagen',					'BDS3241',		'Gol Bolinha')

-- Serviços
INSERT INTO Servico		(descricao, preco_base)
VALUES	('Espelhamento de pintura',		450.00),
		('Vitrificação',				200.00),
		('Martelinho de ouro',			300.00),
		('Limpeza de ar condicionado',	550.00)

-- Orcamento
INSERT INTO Orcamento	(idFuncionario, idCliente, data_criacao, status, total, forma_pgto, parcelas)
VALUES	(1, 2, GETDATE(), 1, 1200, 'PIX', 0) -- Em aberto

INSERT INTO Itens		(idOrcamento, idVeiculo, idServico,	data_entrega, qtd, preco, descricao, taxa, desconto)
VALUES	(1, 1, 1, DATEADD(DAY, 2, GETDATE()), 1, 450, 'Espelhamento de pintura', 0, 50), -- Para entregar em 2 dias após a data atual
		(1, 1, 2, DATEADD(DAY, 2, GETDATE()), 1, 200, 'Vitrificação', 0, 0), -- Para entregar em 2 dias após a data atual
		(1, 1, 3, DATEADD(DAY, 2, GETDATE()), 1, 300, 'Martelinho de ouro', 0, 100), -- Para entregar em 2 dias após a data atual
		(1, 1, 4, DATEADD(DAY, 2, GETDATE()), 1, 550, 'Limpeza de ar condicionado', 0, 150) -- Para entregar em 2 dias após a data atual