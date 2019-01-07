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

namespace Tests
{
    public class RecommendationsControllerTests
    {
        RecommendationsController controller;
        Mock<IRecommendationsService> recommendationService = new Mock<IRecommendationsService>();

        #region Setup Controller context
        [SetUp]
        public void Initialization()
        {
            controller = new RecommendationsController(recommendationService.Object);
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
        public void Recommendations_ReturnsResponseWithDefultValues()
        {
            controller.ModelState.AddModelError("", "");

            var result = controller.NewRecommendations(It.IsAny<NewRecommendationRequest>());

            Assert.AreEqual(0, result.RecommendationId);
            Assert.AreEqual(0, result.VariationId);
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
            recommendationService.Setup(r => r.RecommendationsByUrl("https://developer.mozilla.org/en-US/")).Returns(GetListOfRecommendations().Where(i => i.Url == "https://developer.mozilla.org/en-US/").ToList());

            var result = controller.Recommendations("https://developer.mozilla.org/en-US/");

            Assert.IsTrue(result.All(u => u.Url == "https://developer.mozilla.org/en-US/"));
        }

        [Test]
        public void Recommendations_ReturnsNoRecommendationsByUrl()
        {
            recommendationService.Setup(r => r.RecommendationsByUrl("https://deals.maketecheasier.com/")).Returns(GetListOfRecommendations().Where(i => i.Url == "https://deals.maketecheasier.com/").ToList());

            var result = controller.Recommendations("https://deals.maketecheasier.com/");

            Assert.IsTrue(result.Count == 0);
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
            recommendations.Add(new Recommendations()
            {
                Name = "recomm3",
                Url = "https://www.v2solutions.com/blogs/",
                Description = "",
                Variations = GetListOfVariations().Where(r => r.RecommendationId == 3).ToList(),
                Id = 3
            });
            recommendations.Add(new Recommendations()
            {
                Name = "recomm4",
                Url = "https://developer.mozilla.org/en-US/",
                Description = "",
                Variations = GetListOfVariations().Where(r => r.RecommendationId == 4).ToList(),
                Id = 4
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
                // new Variations(){Id=1, RecommendationId=1, Html="New content 1", DocumentPath="<div><div>Old content 1</div></div>"},
                // new Variations(){Id=2, RecommendationId=1, Html="New content 2", DocumentPath="<div><div>Old content 2</div></div>"},
                // new Variations(){Id=1, RecommendationId=2, Html="New content 12", DocumentPath="<div><div>Old content 12</div></div>"},
                // new Variations(){Id=2, RecommendationId=2, Html="New content 22", DocumentPath="<div><div>Old content 22</div></div>"},
                // new Variations(){Id=3, RecommendationId=2, Html="New content 32", DocumentPath="<div><div>Old content 32</div></div>"},
                // new Variations(){Id=1, RecommendationId=3, Html="New content 13", DocumentPath="<div><div>Old content 13</div></div>"},
                // new Variations(){Id=2, RecommendationId=3, Html="New content 23", DocumentPath="<div><div>Old content 23</div></div>"},
                // new Variations(){Id=3, RecommendationId=3, Html="New content 33", DocumentPath="<div><div>Old content 33</div></div>"},
                // new Variations(){Id=4, RecommendationId=3, Html="New content 43", DocumentPath="<div><div>Old content 43</div></div>"},
                // new Variations(){Id=1, RecommendationId=4, Html="New content 14", DocumentPath="<div><div>Old content 14</div></div>"},
                // new Variations(){Id=2, RecommendationId=4, Html="New content 24", DocumentPath="<div><div>Old content 24</div></div>"},

                new Variations(){Id=1, RecommendationId=1},
                new Variations(){Id=2, RecommendationId=1},
                new Variations(){Id=1, RecommendationId=2},
                new Variations(){Id=2, RecommendationId=2},
                new Variations(){Id=3, RecommendationId=2},
                new Variations(){Id=1, RecommendationId=3},
                new Variations(){Id=2, RecommendationId=3},
                new Variations(){Id=3, RecommendationId=3},
                new Variations(){Id=4, RecommendationId=3},
                new Variations(){Id=1, RecommendationId=4},
                new Variations(){Id=2, RecommendationId=4}
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