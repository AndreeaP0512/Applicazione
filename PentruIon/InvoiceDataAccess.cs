using System.Data.SqlClient;
using System.Data;

namespace PentruIon
{
  class InvoiceDataAccess
  {
    private SqlDataAdapter currencyDA;

    private SqlDataAdapter currencyRateDA;

    private SqlDataAdapter invoiceDA;

    private SqlDataAdapter invoiceItemDA;

    private SqlConnectionStringBuilder _connectionStringBuilder;

    private SqlConnection _connection;

    public SqlCommand CreateCommand(string cmdText)
    {
      SqlCommand sqlCommand = new SqlCommand(cmdText);
      sqlCommand.CommandType = CommandType.StoredProcedure;
      sqlCommand.Connection = _connection;
      return sqlCommand;
    }

    public InvoiceDataAccess()
    {
      Initialize();
    }

    public void Initialize()
    {
      _connection = new SqlConnection();
      _connectionStringBuilder = new SqlConnectionStringBuilder(
        "Persist Security Info = False; Integrated Security = true; Initial Catalog = Currency; Server = ANDREEAP\\SQLEXPRESSLUCA ");
      _connection.ConnectionString = _connectionStringBuilder.ConnectionString;

      #region currencyDA

      currencyDA = new SqlDataAdapter();

      currencyDA.SelectCommand = CreateCommand("ProcCurrencySelect");
      currencyDA.SelectCommand.CommandType = CommandType.StoredProcedure;

      currencyDA.InsertCommand = CreateCommand("ProcCurrencyInsert");
      currencyDA.InsertCommand.CommandType = CommandType.StoredProcedure;
      currencyDA.InsertCommand.Parameters.Add("@Id", SqlDbType.Int, 0, "Id");
      currencyDA.InsertCommand.Parameters.Add("@ShortName", SqlDbType.VarChar, 20, "ShortName");
      currencyDA.InsertCommand.Parameters.Add("@Symbol", SqlDbType.VarChar, 1, "Symbol");
      currencyDA.InsertCommand.Parameters.Add("@IsReference", SqlDbType.Bit, 0, "IsReference");
      currencyDA.InsertCommand.Parameters["@Id"].Direction = ParameterDirection.Output;

      currencyDA.UpdateCommand = CreateCommand("ProcCurrencyUpdate");
      currencyDA.UpdateCommand.CommandType = CommandType.StoredProcedure;
      currencyDA.UpdateCommand.Parameters.Add("@Id", SqlDbType.Int, 0, "Id");
      currencyDA.UpdateCommand.Parameters.Add("@ShortName", SqlDbType.VarChar, 20, "ShortName");
      currencyDA.UpdateCommand.Parameters.Add("@Symbol", SqlDbType.VarChar, 1, "Symbol");
      currencyDA.UpdateCommand.Parameters.Add("@IsReference", SqlDbType.Bit, 0, "IsReference");

      currencyDA.DeleteCommand = CreateCommand("ProcCurrencyDelete");
      currencyDA.DeleteCommand.CommandType = CommandType.StoredProcedure;
      currencyDA.DeleteCommand.Parameters.Add("@Id", SqlDbType.Int, 0, "Id");

      #endregion currencyDA 

      #region currencyRateDA 

      currencyRateDA = new SqlDataAdapter();

      currencyRateDA.SelectCommand = CreateCommand("ProcCurrencyRateSelect");
      currencyRateDA.SelectCommand.CommandType = CommandType.StoredProcedure;

      currencyRateDA.InsertCommand = CreateCommand("ProcCurrencyRateInsert");
      currencyRateDA.InsertCommand.CommandType = CommandType.StoredProcedure;
      currencyRateDA.InsertCommand.Parameters.Add("@Id", SqlDbType.Int, 0, "Id");
      currencyRateDA.InsertCommand.Parameters.Add("@ExchangeDate", SqlDbType.DateTime, 0, "ExchangeDate");
      currencyRateDA.InsertCommand.Parameters.Add("@ExchangeRate", SqlDbType.Decimal, 0, "ExchangeRate");
      currencyRateDA.InsertCommand.Parameters.Add("@IdCurrency", SqlDbType.Int, 0, "IdCurrency");
      currencyRateDA.InsertCommand.Parameters["@Id"].Direction = ParameterDirection.Output;

      currencyRateDA.UpdateCommand = CreateCommand("ProcCurrencyRateUpdate");
      currencyRateDA.UpdateCommand.CommandType = CommandType.StoredProcedure;
      currencyRateDA.UpdateCommand.Parameters.Add("@Id", SqlDbType.Int, 0, "Id");
      currencyRateDA.UpdateCommand.Parameters.Add("@ExchangeDate", SqlDbType.DateTime, 0, "ExchangeDate");
      currencyRateDA.UpdateCommand.Parameters.Add("@ExchangeRate", SqlDbType.Decimal, 0, "ExchangeRate");
      currencyRateDA.UpdateCommand.Parameters.Add("@IdCurrency", SqlDbType.Int, 0, "IdCurrency");

      currencyRateDA.DeleteCommand = CreateCommand("ProcCurrencyRateDelete");
      currencyRateDA.DeleteCommand.CommandType = CommandType.StoredProcedure;
      currencyRateDA.DeleteCommand.Parameters.Add("@Id", SqlDbType.Int, 0, "Id");

      #endregion currencyRateDA

      #region invoiceDA 

      invoiceDA = new SqlDataAdapter();

      invoiceDA.SelectCommand = CreateCommand("ProcInvoiceSelect");
      invoiceDA.SelectCommand.CommandType = CommandType.StoredProcedure;

      invoiceDA.InsertCommand = CreateCommand("ProcInvoiceInsert");
      invoiceDA.InsertCommand.CommandType = CommandType.StoredProcedure;
      invoiceDA.InsertCommand.Parameters.Add("@Id", SqlDbType.Int, 0, "Id");
      invoiceDA.InsertCommand.Parameters.Add("@InvoiceNumber", SqlDbType.VarChar, 20, "InvoiceNumber");
      invoiceDA.InsertCommand.Parameters.Add("@ClientName", SqlDbType.VarChar, 30, "ClientName");
      invoiceDA.InsertCommand.Parameters.Add("@IssueDate", SqlDbType.DateTime, 0, "IssueDate");
      invoiceDA.InsertCommand.Parameters.Add("@DueDate", SqlDbType.DateTime, 0, "DueDate");
      invoiceDA.InsertCommand.Parameters.Add("@IdCurrency", SqlDbType.Int, 0, "IdCurrency");
      invoiceDA.InsertCommand.Parameters["@Id"].Direction = ParameterDirection.Output;

      invoiceDA.UpdateCommand = CreateCommand("ProcInvoiceUpdate");
      invoiceDA.UpdateCommand.CommandType = CommandType.StoredProcedure;
      invoiceDA.UpdateCommand.Parameters.Add("@Id", SqlDbType.Int, 0, "Id");
      invoiceDA.UpdateCommand.Parameters.Add("@InvoiceNumber", SqlDbType.VarChar, 20, "InvoiceNumber");
      invoiceDA.UpdateCommand.Parameters.Add("@ClientName", SqlDbType.VarChar, 30, "ClientName");
      invoiceDA.UpdateCommand.Parameters.Add("@IssueDate", SqlDbType.DateTime, 0, "IssueDate");
      invoiceDA.UpdateCommand.Parameters.Add("@DueDate", SqlDbType.DateTime, 0, "DueDate");
      invoiceDA.UpdateCommand.Parameters.Add("@IdCurrency", SqlDbType.Int, 0, "IdCurrency");

      invoiceDA.DeleteCommand = CreateCommand("ProcInvoiceDelete");
      invoiceDA.DeleteCommand.CommandType = CommandType.StoredProcedure;
      invoiceDA.DeleteCommand.Parameters.Add("@Id", SqlDbType.Int, 0, "Id");

      #endregion invoiceDA

      #region invoiceItemDA

      invoiceItemDA = new SqlDataAdapter();

      invoiceItemDA.SelectCommand = CreateCommand("ProcInvoiceItemSelect");
      invoiceItemDA.SelectCommand.CommandType = CommandType.StoredProcedure;

      invoiceItemDA.InsertCommand = CreateCommand("ProcInvoiceItemInsert");
      invoiceItemDA.InsertCommand.CommandType = CommandType.StoredProcedure;
      invoiceItemDA.InsertCommand.Parameters.Add("@Id", SqlDbType.Int, 0, "Id");
      invoiceItemDA.InsertCommand.Parameters.Add("@ProductName", SqlDbType.VarChar, 20, "ProductName");
      invoiceItemDA.InsertCommand.Parameters.Add("@Quantity", SqlDbType.Int, 0, "Quantity");
      invoiceItemDA.InsertCommand.Parameters.Add("@Price", SqlDbType.Decimal, 0, "Price");
      invoiceItemDA.InsertCommand.Parameters.Add("@Value", SqlDbType.Decimal, 0, "Value");
      invoiceItemDA.InsertCommand.Parameters.Add("@IdInvoice", SqlDbType.Int, 0, "IdInvoice");
      invoiceItemDA.InsertCommand.Parameters["@Id"].Direction = ParameterDirection.Output;

      invoiceItemDA.UpdateCommand = CreateCommand("ProcInvoiceItemUpdate");
      invoiceItemDA.UpdateCommand.CommandType = CommandType.StoredProcedure;
      invoiceItemDA.UpdateCommand.Parameters.Add("@Id", SqlDbType.Int, 0, "Id");
      invoiceItemDA.UpdateCommand.Parameters.Add("@ProductName", SqlDbType.VarChar, 20, "ProductName");
      invoiceItemDA.UpdateCommand.Parameters.Add("@Quantity", SqlDbType.Int, 0, "Quantity");
      invoiceItemDA.UpdateCommand.Parameters.Add("@Price", SqlDbType.Decimal, 0, "Price");
      invoiceItemDA.UpdateCommand.Parameters.Add("@Value", SqlDbType.Decimal, 0, "Value");
      invoiceItemDA.UpdateCommand.Parameters.Add("@IdInvoice", SqlDbType.Int, 0, "IdInvoice");

      invoiceItemDA.DeleteCommand = CreateCommand("ProcInvoiceItemDelete");
      invoiceItemDA.DeleteCommand.CommandType = CommandType.StoredProcedure;
      invoiceItemDA.DeleteCommand.Parameters.Add("@Id", SqlDbType.Int, 0, "Id");

      #endregion invoiceItemDA

    }

    public InvoiceDS ReadInvoiceData()
    {
      InvoiceDS invoiceDS = new InvoiceDS();
      try
      {
        _connection.Open();
        currencyDA.Fill(invoiceDS.Currency);
        invoiceDA.Fill(invoiceDS.Invoice);
        currencyRateDA.Fill(invoiceDS.CurrencyRate);
        invoiceItemDA.Fill(invoiceDS.InvoiceItem);
      }
      catch
      {

      }
      finally
      {
        _connection.Close();
      }

      return invoiceDS;
    }

    public void WriteInvoiceData(InvoiceDS invoiceDS)
    {
      invoiceItemDA.Update(invoiceDS.InvoiceItem.Select(null, null, DataViewRowState.Deleted));
      invoiceDA.Update(invoiceDS.Invoice.Select(null, null, DataViewRowState.Deleted));
      currencyRateDA.Update(invoiceDS.CurrencyRate.Select(null, null, DataViewRowState.Deleted));
      currencyDA.Update(invoiceDS.Currency.Select(null, null, DataViewRowState.Deleted));


      currencyDA.Update(invoiceDS.Currency.Select(null, null, DataViewRowState.ModifiedCurrent));
      currencyDA.Update(invoiceDS.Currency.Select(null, null, DataViewRowState.Added));

      currencyRateDA.Update(invoiceDS.CurrencyRate.Select(null, null, DataViewRowState.ModifiedCurrent));
      currencyRateDA.Update(invoiceDS.CurrencyRate.Select(null, null, DataViewRowState.Added));

      invoiceDA.Update(invoiceDS.Invoice.Select(null, null, DataViewRowState.ModifiedCurrent));
      invoiceDA.Update(invoiceDS.Invoice.Select(null, null, DataViewRowState.Added));

      invoiceItemDA.Update(invoiceDS.InvoiceItem.Select(null, null, DataViewRowState.ModifiedCurrent));
      invoiceItemDA.Update(invoiceDS.InvoiceItem.Select(null, null, DataViewRowState.Added));

    }

  }
}
