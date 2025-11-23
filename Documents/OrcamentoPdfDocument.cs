using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using AfReparosAutomotivos.Models;

public class OrcamentoPdfDocument : IDocument
{
    private readonly Orcamentos Orcamento;
    private readonly Clientes Cliente;
    private readonly Veiculos Veiculo;
    private readonly IEnumerable<Item> Itens;

    public OrcamentoPdfDocument(
        Orcamentos orcamento,
        Clientes cliente,
        Veiculos veiculo,
        IEnumerable<Item> itens)
    {
        Orcamento = orcamento;
        Cliente = cliente;
        Veiculo = veiculo;
        Itens = itens;
    }

    public DocumentMetadata GetMetadata() => new DocumentMetadata();

    public DocumentSettings GetSettings() => new DocumentSettings();

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Margin(30);

            page.Header().Element(ComposeHeader);
            page.Content().Element(ComposeContent);
            page.Footer().AlignCenter().Text("Obrigado pela preferência!");
        });
    }

    private void ComposeHeader(IContainer container)
    {
        container.Row(row =>
        {
            row.RelativeItem().Column(col =>
            {
                col.Item().Text("ORÇAMENTO DE SERVIÇO")
                    .FontSize(20).Bold().AlignCenter();
                col.Item().Text("\n");
            });
        });
    }

    private void ComposeContent(IContainer container)
    {
        container.Column(col =>
        {
            col.Item().Element(ComposeCliente);
            col.Item().PaddingTop(10).Element(ComposeVeiculo);
            col.Item().PaddingTop(10).Element(ComposeItens);
            col.Item().PaddingTop(20).Element(ComposeTotal);
        });
    }

    private void ComposeCliente(IContainer container)
    {
        container.Column(col =>
        {
            col.Item().Text("Dados do Cliente")
                .FontSize(14).Bold();

            col.Item().Table(table =>
            {
                table.ColumnsDefinition(c =>
                {
                    c.RelativeColumn(1);
                    c.RelativeColumn(3);
                });

                table.Cell().Text("Nome").Bold();
                table.Cell().Text(Cliente.nome);

                table.Cell().Text("Documento").Bold();
                table.Cell().Text(Cliente.documento ?? "");

                table.Cell().Text("Telefone").Bold();
                table.Cell().Text(Cliente.telefone ?? "");

                table.Cell().Text("Endereço").Bold();
                table.Cell().Text(Cliente.endereco ?? "");
            });
        });
    }

    private void ComposeVeiculo(IContainer container)
    {
        var marca = Veiculo?.marca ?? "-";
        var modelo = Veiculo?.modelo ?? "-";
        var placa = Veiculo?.placa ?? "-";

        container.Column(col =>
        {
            col.Item().Text("Dados do Veículo")
                .Bold().FontSize(14);

            col.Item().Table(table =>
            {
                table.ColumnsDefinition(c =>
                {
                    c.RelativeColumn(1);
                    c.RelativeColumn(3);
                });

                table.Cell().Text("Marca").Bold();
                table.Cell().Text(Veiculo.marca);

                table.Cell().Text("Modelo").Bold();
                table.Cell().Text(Veiculo.modelo);

                table.Cell().Text("Placa").Bold();
                table.Cell().Text(Veiculo.placa);
            });
        });
    }

    private void ComposeItens(IContainer container)
    {
        container.Column(col =>
        {
            col.Item().Text("Serviços / Itens")
                .Bold().FontSize(14);

            col.Item().Table(table =>
            {
                table.ColumnsDefinition(c =>
                {
                    c.RelativeColumn(4);
                    c.RelativeColumn(1);
                    c.RelativeColumn(2);
                    c.RelativeColumn(2);
                });

                // Header
                table.Header(header =>
                {
                    header.Cell().Element(CellHeader).Text("Descrição");
                    header.Cell().Element(CellHeader).Text("Qtd");
                    header.Cell().Element(CellHeader).Text("Valor Unit.");
                    header.Cell().Element(CellHeader).Text("Subtotal");
                });

                foreach (var item in Itens)
                {
                    // decimal valor_com_desconto = (item.preco ?? 0m) - (item.desconto ?? 0m);
                    // var subtotal = item.qtd * valor_com_desconto;
                    decimal desconto = item.desconto ?? 0m;
                    decimal valor_final = item.preco - desconto;
                    decimal subtotal = item.qtd * valor_final;

                    table.Cell().Element(CellDefault).Text(item.descricao);
                    table.Cell().Element(CellDefault).Text(item.qtd.ToString());
                    table.Cell().Element(CellDefault).Text(item.preco.ToString("C"));
                    table.Cell().Element(CellDefault).Text(subtotal.ToString("C"));
                }
            });
        });
    }

    private static IContainer CellHeader(IContainer container) =>
        container.Padding(5).Background("#EEE").Border(0.5f).BorderColor("#999");

    private static IContainer CellDefault(IContainer container) =>
        container.Padding(5).BorderBottom(0.5f).BorderColor("#DDD");

    private void ComposeTotal(IContainer container)
    {
        decimal total = Itens.Sum(i => i.qtd * (i.preco - (i.desconto ?? 0m)));

        container.AlignRight().Text($"TOTAL: {total:C}")
            .FontSize(16).Bold();
    }
}
