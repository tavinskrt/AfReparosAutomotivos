create procedure cadCli
(
	@nome varchar(50), @documento varchar(14), @endereco varchar(200), @telefone varchar(14)
)
as
begin
	begin try

		declare @codigo int

		begin tran -- ***********
		select @codigo = idPessoa from Pessoa
		where idPessoa = @documento -- buscando a pessoa pelo documento
	
		if @@ROWCOUNT = 0 -- pessoa não cadastrada
		begin
			declare @tipo_pes varchar(1)
			if 

			insert into Pessoa values (@nome, @telefone, @endereco, @documento, 1)
			insert into clientes values (@@IDENTITY, @renda, @renda*0.25)
			commit -- ***********
			return 0 -- cliente cadastrado
		end
		else
		begin
			-- pessoa já está cadastrada
			if not exists (select * from clientes where pes_codigo = @codigo)
			begin
				insert into clientes values (@codigo, @renda, @renda * 0.25)
				commit -- ***********
				return 0 -- cliente cadastrado
			end
			else
			begin
				rollback -- ***********
				return 1 -- cliente já estava cadastrado
			end -- fim else
		end -- fim else
	end try 
	begin catch
		rollback
		raiserror ('Problemas no cadastro do cliente', 16, 1)
		return 2 -- erro
	end catch
end
go
