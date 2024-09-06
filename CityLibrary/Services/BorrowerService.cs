using Azure;
using Azure.Data.Tables;
using CityLibrary.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CityLibrary.Services
{
    public class BorrowerService
    {
        private readonly TableClient _tableClient;

        public BorrowerService(string storageConnectionString, string tableName)
        {
            var serviceClient = new TableServiceClient(storageConnectionString);
            _tableClient = serviceClient.GetTableClient(tableName);
            _tableClient.CreateIfNotExists();
        }

        // Add a new borrower
        public async Task AddBorrowerAsync(Borrower borrower)
        {
            borrower.UpdateTotal(); // Ensure the total is calculated
            await _tableClient.AddEntityAsync(borrower);
        }

        // Get a borrower by PartitionKey and RowKey
        public async Task<Borrower> GetBorrowerAsync(string partitionKey, string rowKey)
        {
            try
            {
                return await _tableClient.GetEntityAsync<Borrower>(partitionKey, rowKey);
            }
            catch (RequestFailedException)
            {
                return null; // Return null if not found
            }
        }

        // Retrieve all borrowers
        public async Task<List<Borrower>> GetAllBorrowersAsync()
        {
            var borrowers = new List<Borrower>();
            await foreach (var entity in _tableClient.QueryAsync<Borrower>())
            {
                entity.UpdateTotal(); // Ensure Total is updated
                borrowers.Add(entity);
            }
            return borrowers;
        }

        // Update a borrower
        public async Task UpdateBorrowerAsync(Borrower borrower)
        {
            borrower.UpdateTotal(); // Ensure the total is updated
            await _tableClient.UpdateEntityAsync(borrower, ETag.All, TableUpdateMode.Replace);
        }

        // Delete a borrower
        public async Task DeleteBorrowerAsync(string partitionKey, string rowKey)
        {
            await _tableClient.DeleteEntityAsync(partitionKey, rowKey);
        }
    }
}
