using Moq;
using NUnit.Framework;
using RWRAP_API.Controllers;
using RWRAP_API.Models;
using RWRAP_API.Services;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using RWRAP_API.Attributes;
using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace Tests
{
    public class RecommendationsControllerTests
    {
        RecommendationsController controller;
        Mock<IRecommendationsService> recommendationService = new Mock<IRecommendationsService>();
        Mock<ILogger<RecommendationsController>> logger = new Mock<ILogger<RecommendationsController>>();

        #region Setup Controller context
        [SetUp]
        public void Initialization()
        {
            controller = new RecommendationsController(recommendationService.Object, logger.Object);
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.Request.Scheme = "https";
        }
        #endregion Setup Controller context

        #region Verify validations

        [Test]
        public void VerifyRequiredAttribute()
        {
            var result = CheckValidation(new NewRecommendationRequest { Name = "", Url = "" });

            Assert.IsTrue(result.Exists(e => e.ErrorMessage == "Name is required"));
            Assert.IsTrue(result.Exists(e => e.ErrorMessage == "Url is required"));
        }

        [Test]
        public void VerifyUrlAttribute()
        {
            var result = CheckValidation(new NewRecommendationRequest { Name = "", Url = "google.com" });
            Assert.IsTrue(result.Exists(e => e.ErrorMessage == "It is not a fully qualified url"));
        }

        #endregion Verify validations

        #region Create recommendations

        [Test]
        public void Recommendations_ReturnsResponseWithIds()
        {
            recommendationService.Setup(s => s.NewRecommendations(It.Is<NewRecommendationRequest>(u => u.Url == "https://www.v2solutions.com/blogs/"), It.Is<string>(i => i == "https"))).Returns(new NewRecommendationResponse() { RecommendationId = 1, VariationId = 2 });

            var result = controller.NewRecommendations(new NewRecommendationRequest() { Url = "https://www.v2solutions.com/blogs/" });

            Assert.AreEqual(1, result.RecommendationId);
            Assert.AreEqual(2, result.VariationId);
        }

        [Test]
        public void Recommendations_ReturnsNullResponse()
        {
            controller.ModelState.AddModelError("", "");

            var result = controller.NewRecommendations(It.IsAny<NewRecommendationRequest>());

            Assert.IsNull(result);
        }

        #endregion Create recommendations

        #region Retrive all recommendation

        [Test]
        public void Recommendations_ReturnsRecommendations()
        {
            recommendationService.Setup(s => s.AllRecommendations()).Returns(GetListOfRecommendations());

            var result = controller.Recommendations();

            Assert.IsTrue(result.Count >= 0);
        }

        [Test]
        public void Recommendations_ReturnsNoRecommendations()
        {
            recommendationService.Setup(s => s.AllRecommendations()).Returns(new List<Recommendations>());

            var result = controller.Recommendations();

            Assert.IsTrue(result.Count == 0);
        }

        #endregion Retrive all recommendation

        #region Retrive recommendations by url

        [Test]
        public void Recommendations_ReturnsRecommendationsByUrl()
        {
            recommendationService.Setup(r => r.RecommendationsByUrl("https://developer.mozilla.org/en-US/")).Returns(GetListOfRecommendations().Where(i => i.Url == "https://developer.mozilla.org/en-US/").SingleOrDefault());

            var result = controller.Recommendations("https://developer.mozilla.org/en-US/");

            Assert.IsTrue(result.Url == "https://developer.mozilla.org/en-US/");
        }

        [Test]
        public void Recommendations_ReturnsNoRecommendationsByUrl()
        {
            recommendationService.Setup(r => r.RecommendationsByUrl("https://deals.maketecheasier.com/")).Returns(GetListOfRecommendations().Where(i => i.Url == "https://deals.maketecheasier.com/").SingleOrDefault());

            var result = controller.Recommendations("https://deals.maketecheasier.com/");

            Assert.IsNull(result);
        }

        #endregion Retrive recommendations by url

        #region Retrive recommendations by id

        [Test]
        public void Recommendations_ReturnsRecommendation()
        {
            recommendationService.Setup(r => r.RecommendationById(3)).Returns(GetRecommendation());

            var result = controller.Recommendations(3);

            Assert.IsTrue(result.Id == 3);
        }

        [Test]
        public void Recommendations_ReturnsNullRecommendation()
        {
            var result = controller.Recommendations(6);

            Assert.IsNull(result);
        }

        #endregion Retrive recommendations by id

        #region Dummy recommendatons model

        private List<Recommendations> GetListOfRecommendations()
        {
            List<Recommendations> recommendations = new List<Recommendations>();
            recommendations.Add(new Recommendations()
            {
                Name = "recomm1",
                Url = "https://www.v2solutions.com/blogs/",
                Description = "",
                Variations = GetListOfVariations().Where(r => r.RecommendationId == 1).ToList(),
                Id = 1
            });
            recommendations.Add(new Recommendations()
            {
                Name = "recomm2",
                Url = "https://developer.mozilla.org/en-US/",
                Description = "",
                Variations = GetListOfVariations().Where(r => r.RecommendationId == 2).ToList(),
                Id = 2
            });

            return recommendations;
        }

        private NewRecommendation GetRecommendation()
        {
            return new NewRecommendation()
            {
                Html = "",
                Name = "recomm3",
                Url = "https://www.v2solutions.com/blogs/",
                Description = "",
                Variations = GetListOfVariations().Where(r => r.RecommendationId == 3).ToList(),
                Id = 3
            };
        }
        #endregion Dummy recommendatons model

        private List<Variations> GetListOfVariations()
        {
            return new List<Variations>(){
                new Variations(){Id=1, Name ="variation1", RecommendationId=1, DynamicContent=""},
                new Variations(){Id=2, Name ="variation2", RecommendationId=1, DynamicContent=""},
                new Variations(){Id=12, Name ="variation12", RecommendationId=2, DynamicContent=""},
                new Variations(){Id=22, Name ="variation22", RecommendationId=2, DynamicContent=""},
                new Variations(){Id=32, Name ="variation32", RecommendationId=2, DynamicContent=""},
                new Variations(){Id=13, Name ="variation13", RecommendationId=3, DynamicContent=""},
                new Variations(){Id=23, Name ="variation23", RecommendationId=3, DynamicContent=""},
                new Variations(){Id=33, Name ="variation33", RecommendationId=3, DynamicContent=""},
                new Variations(){Id=43, Name ="variation43", RecommendationId=3, DynamicContent=""},
                new Variations(){Id=14, Name ="variation14", RecommendationId=4, DynamicContent=""},
                new Variations(){Id=24, Name ="variation24", RecommendationId=4, DynamicContent=""}
            };
        }

        #region Private method to call runtime validations(Model validations)
        private List<ValidationResult> CheckValidation(object model)
        {
            var result = new List<ValidationResult>();
            var validationContext = new ValidationContext(model);
            Validator.TryValidateObject(model, validationContext, result, true);
            if (model is IValidatableObject)
                (model as IValidatableObject).Validate(validationContext);

            return result;
        }
        #endregion Private method to call runtime validations(Model validations)
    }
}