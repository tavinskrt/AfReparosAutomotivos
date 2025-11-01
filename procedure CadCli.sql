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
		where documento = @documento -- buscando a pessoa pelo documento
	
		if @@ROWCOUNT = 0 -- pessoa n�o cadastrada
		begin
			declare @tipo_pes varchar(1)
			if LEN(@documento) = 14
			begin
				set @tipo_pes = 'J'
			end
			else
			begin
				set @tipo_pes = 'F'
			end 
			insert into Pessoa values (@nome, @telefone, @endereco, @documento, @tipo_pes)
			insert into Cliente values (@@IDENTITY)
			commit -- ***********
			return 0 -- cliente cadastrado
		end
		else
		begin
			-- pessoa j� est� cadastrada
			if not exists (select * from Cliente where idCliente = @codigo)
			begin
				insert into Cliente values (@codigo)
				commit -- ***********
				return 0 -- cliente cadastrado
			end
			else
			begin
				rollback -- ***********
				return 1 -- cliente j� estava cadastrado
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
