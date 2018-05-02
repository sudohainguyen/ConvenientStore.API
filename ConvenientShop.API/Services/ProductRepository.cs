using System.Collections.Generic;
using System.Linq;
using System.Text;
using ConvenientShop.API.Entities;
using ConvenientShop.API.Models;
using Dapper;
using Dapper.Contrib.Extensions;
using Dapper.Mapper;
using Microsoft.Extensions.Options;

namespace ConvenientShop.API.Services
{
    public class ProductRepository : ConvenientStoreRepository, Interfaces.IProductRepository
    {
        public ProductRepository(IOptions<StoreConfig> config) : base(config) { }

        public IEnumerable<ProductDetail> GetAllDetailForProduct(int productId)
        {
            using(var conn = Connection)
            {
                conn.Open();
                var sql = "SELECT * FROM product_detail WHERE ProId = @productId";
                return conn.Query<ProductDetail>(sql, param : new { productId });
            }
        }

        public Product GetProduct(int productId, bool includeDetail)
        {
            using(var conn = Connection)
            {
                conn.Open();

                var dict = new Dictionary<int, Product>();

                var sql = new StringBuilder("SELECT * FROM product as p ");
                sql.Append("INNER JOIN supplier as s ON p.SupId = s.SupplierId ");
                sql.Append("INNER JOIN category as c ON p.CateId = c.CategoryId ");
                if (includeDetail)
                {
                    sql.Append("INNER JOIN product_detail as pd ON p.ProductId = pd.ProId ");
                }
                sql.Append("WHERE p.ProductId = @productId");

                return includeDetail ?
                    conn.Query<Product, Supplier, Category, ProductDetail, Product>(
                        sql.ToString(),
                        map: (p, s, c, pd) =>
                        {
                            p.Supplier = s;
                            p.Category = c;

                            if (!dict.TryGetValue(p.ProductId, out var entry))
                            {
                                entry = p;
                                entry.Details = new List<ProductDetail>();
                                dict.Add(entry.ProductId, entry);
                            }
                            p.Details.Add(pd);
                            return p;
                        },
                        param : new { productId },
                        splitOn: "SupplierId, CategoryId, BarCode"
                    ).FirstOrDefault() :
                    conn.Query<Product, Supplier, Category>(
                        sql.ToString(),
                        splitOn: "SupplierId, CategoryId",
                        param : new { productId }
                    ).FirstOrDefault();
            }
        }

        public ProductDetail GetProductDetail(string barcode)
        {
            using(var conn = Connection)
            {
                conn.Open();
                var sql = "SELECT * FROM product_detail AS pd " +
                    "INNER JOIN product AS p ON p.ProductId = pd.ProId " +
                    "INNER JOIN shipment AS s ON s.ShipmentId = pd.ShipmentId " +
                    "WHERE pd.BarCode = @barcode";
                return conn.Query<ProductDetail, Product, Shipment>(
                    sql,
                    splitOn: "ProductId, DeliveryId",
                    param : new { barcode }
                ).FirstOrDefault();
            }
        }

        public IEnumerable<Product> GetProducts()
        {
            using(var conn = Connection)
            {
                conn.Open();
                return conn.GetAll<Product>();
            }
        }

        public bool ProductExists(int productId)
        {
            using(var conn = Connection)
            {
                conn.Open();
                var sql = "SELECT ProductId FROM product WHERE ProductId = @productId";
                return conn.ExecuteScalar(sql, param : new { productId }) != null;
            }
        }
    }
}