using Jahoot.Display.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Jahoot.Display.Services;

public class TestService : ITestService
{
    private readonly IHttpService _httpService;

    public TestService(IHttpService httpService)
    {
        _httpService = httpService;
    }

    public async Task<IEnumerable<UpcomingTestResponse>> GetUpcomingTestsAsync()
    {
        return await _httpService.GetAsync<IEnumerable<UpcomingTestResponse>>("api/student/tests/upcoming") ?? [];
    }

    public async Task<IEnumerable<CompletedTestResponse>> GetCompletedTestsAsync()
    {
        return await _httpService.GetAsync<IEnumerable<CompletedTestResponse>>("api/student/tests/completed") ?? [];
    }

    public async Task<TestDetailsResponse?> GetTestDetailsAsync(int testId)
    {
        return await _httpService.GetAsync<TestDetailsResponse>($"api/test/{testId}");
    }

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
