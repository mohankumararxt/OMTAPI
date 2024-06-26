using OMT.DataAccess.Context;
using OMT.DataAccess.Entities;
using OMT.DataService.Interface;
using OMT.DTO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMT.DataService.Service
{
    public class ProductDescriptionService : IProductDescriptionService
    {
        private readonly OMTDataContext _oMTDataContext;

        public ProductDescriptionService(OMTDataContext oMTDataContext)
        {
            _oMTDataContext = oMTDataContext;
        }

        public ResultDTO CreateProductDescription(string ProductDescriptionName)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };
            try
            {
                var existing_ProductDescription = _oMTDataContext.ProductDescription.Where(x => x.ProductDescriptionName == ProductDescriptionName && x.IsActive).FirstOrDefault();

                if (existing_ProductDescription != null)
                {
                    resultDTO.IsSuccess = false;
                    resultDTO.Message = "Product Description already exists. Please try to add different Product Description";
                }
                else
                {
                    ProductDescription productDescription = new ProductDescription()
                    {
                        ProductDescriptionName = ProductDescriptionName,
                        IsActive = true
                    };

                    _oMTDataContext.ProductDescription.Add(productDescription);
                    _oMTDataContext.SaveChanges();
                    resultDTO.IsSuccess = true;
                    resultDTO.Message = "ProductDescription created successfully";

                }
            }
            catch (Exception ex)    
            {
                resultDTO.IsSuccess = false;
                resultDTO.StatusCode = "500";
                resultDTO.Message = ex.Message;
            }
            return resultDTO;
        }

        public ResultDTO DeleteProductDescription(int id)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };
            try
            {
                ProductDescription productDescription = _oMTDataContext.ProductDescription.Where(x => x.ProductDescriptionId == id && x.IsActive).FirstOrDefault();

                if (productDescription != null)
                {
                    productDescription.IsActive = false;
                    _oMTDataContext.ProductDescription.Update(productDescription);
                    _oMTDataContext.SaveChanges();
                    resultDTO.IsSuccess = true;
                    resultDTO.Message = "Product Description has been deleted successfully";
                }
                else
                {
                    resultDTO.StatusCode = "404";
                    resultDTO.IsSuccess = false;
                    resultDTO.Message = "Product Description is not found";
                }
            }
            catch (Exception ex)
            {
                resultDTO.IsSuccess = false;
                resultDTO.StatusCode = "500";
                resultDTO.Message = ex.Message;
            }
            return resultDTO;
        }

        public ResultDTO GetProductDescription()
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };
            try
            {
                List<ProductDescriptionResponseDTO> productDescriptions = _oMTDataContext.ProductDescription.Where(x => x.IsActive).Select(_ =>new ProductDescriptionResponseDTO { ProductDescriptionId =  _.ProductDescriptionId, ProductDescriptionName = _.ProductDescriptionName}).ToList();

                if (productDescriptions.Count > 0)
                {
                    resultDTO.IsSuccess = true;
                    resultDTO.Message = "List of Product Descriptions";
                    resultDTO.Data = productDescriptions;
                }

            }
            catch (Exception ex) 
            {
                resultDTO.IsSuccess = false;
                resultDTO.StatusCode = "500";
                resultDTO.Message = ex.Message;
            }
            return resultDTO;
        }

        public ResultDTO UpdateProductDescription(ProductDescriptionResponseDTO productDescriptionResponseDTO)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "201" };
            try
            {
                ProductDescription productDescription = _oMTDataContext.ProductDescription.Where(x => x.ProductDescriptionId == productDescriptionResponseDTO.ProductDescriptionId && x.IsActive).FirstOrDefault();

                if (productDescription != null)
                {
                    productDescription.ProductDescriptionName = productDescriptionResponseDTO.ProductDescriptionName;
                    _oMTDataContext.ProductDescription.Update(productDescription);
                    _oMTDataContext.SaveChanges();
                    resultDTO.IsSuccess = true;
                    resultDTO.Message = "Product Description updated successfully";
                }
                else
                {
                    resultDTO.IsSuccess = false;
                    resultDTO.Message = "Product Description not found";
                    resultDTO.StatusCode = "404";
                }
            }
            catch (Exception ex)
            {
                resultDTO.IsSuccess = false;
                resultDTO.StatusCode = "500";
                resultDTO.Message = ex.Message;
            }
            return resultDTO;
        }
    }
}
