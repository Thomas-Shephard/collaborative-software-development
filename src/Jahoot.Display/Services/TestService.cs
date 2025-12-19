using Jahoot.Core.Models;
using Jahoot.Core.Models.Requests;
using Jahoot.WebApi.Models.Responses;
using Jahoot.Display.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Jahoot.Display.Services
{
    public class TestService : ITestService
    {
        private readonly IHttpService _httpService;

        public TestService(IHttpService httpService)
        {
            _httpService = httpService;
        }

        public async Task<IEnumerable<Test>> GetTests(int? subjectId = null)
        {
            string uri = "api/test/list";
            if (subjectId.HasValue)
            {
                uri += $"?subjectId={subjectId.Value}";
            }

            IEnumerable<Test>? result = await _httpService.GetAsync<IEnumerable<Test>>(uri);
            return result ?? Enumerable.Empty<Test>();
        }

        public async Task<Test?> GetTestById(int testId)
        {
            return await _httpService.GetAsync<Test>($"api/test/{testId}");
        }

        public async Task<IEnumerable<CompletedTestResponse>> GetRecentCompletedTests(int? days = null)
        {
            string uri = "api/lecturer/recent-activity";
            if (days.HasValue)
            {
                uri += $"?days={days.Value}";
            }

            IEnumerable<CompletedTestResponse>? result = await _httpService.GetAsync<IEnumerable<CompletedTestResponse>>(uri);
            return result ?? Enumerable.Empty<CompletedTestResponse>();
        }

        public async Task<Result> CreateTest(Test test)
        {
            var request = new TestRequestModel
            {
                SubjectId = test.SubjectId,
                Name = test.Name,
                NumberOfQuestions = test.NumberOfQuestions,
                Questions = test.Questions.Select(q => new QuestionRequestModel
                {
                    Text = q.Text,
                    Options = q.Options.Select(o => new QuestionOptionRequestModel
                    {
                        OptionText = o.OptionText,
                        IsCorrect = o.IsCorrect
                    }).ToList()
                }).ToList()
            };

            return await _httpService.PostAsync("api/test", request);
        }

        public async Task<Result> UpdateTest(int testId, Test test)
        {
            var request = new TestRequestModel
            {
                SubjectId = test.SubjectId,
                Name = test.Name,
                NumberOfQuestions = test.NumberOfQuestions,
                Questions = test.Questions.Select(q => new QuestionRequestModel
                {
                    Text = q.Text,
                    Options = q.Options.Select(o => new QuestionOptionRequestModel
                    {
                        OptionText = o.OptionText,
                        IsCorrect = o.IsCorrect
                    }).ToList()
                }).ToList()
            };

            return await _httpService.PutAsync($"api/test/{testId}", request);
        }

        public async Task<Result> DeleteTest(int testId)
        {
            return await _httpService.DeleteAsync($"api/test/{testId}");
        }

        public async Task<bool> HasAttempts(int testId)
        {
            HasAttemptsResponse? result = await _httpService.GetAsync<HasAttemptsResponse>($"api/test/{testId}/has-attempts");
            return result?.HasAttempts ?? false;
        }

        public async Task<IEnumerable<UpcomingTestResponse>> GetUpcomingTestsAsync()
        {
            return await _httpService.GetAsync<IEnumerable<UpcomingTestResponse>>("api/student/tests/upcoming") ?? Enumerable.Empty<UpcomingTestResponse>();
        }

        public async Task<IEnumerable<CompletedTestResponse>> GetCompletedTestsAsync()
        {
            return await _httpService.GetAsync<IEnumerable<CompletedTestResponse>>("api/student/tests/completed") ?? Enumerable.Empty<CompletedTestResponse>();
        }

        public async Task<TestDetailsResponse?> GetTestDetailsAsync(int testId)
        {
            return await _httpService.GetAsync<TestDetailsResponse>($"api/test/{testId}");
        }

        //TODO: This method is not compatible with the current IHttpService interface.
        public async Task<TestSubmissionResponse?> SubmitTestAsync(int testId, Dictionary<int, int> answers)
        {
             var answersList = answers.Select(kvp => new
             {
                 QuestionId = kvp.Key,
                 SelectedOptionId = kvp.Value
             }).ToList();

             var request = new { Answers = answersList };
            
             return await _httpService.PostAsync<object, TestSubmissionResponse>($"api/test/{testId}/submit", request);
        }
    }
}