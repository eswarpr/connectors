/// <summary>
/// Represents the script for Regular Expressions connector
/// </summary>
public class Script : ScriptBase
{
    /// <summary>
    /// Cached instance of the JSON serializer
    /// </summary>
    private readonly JsonSerializer serializer;

    /// <summary>
    /// Initialises a new instance of the script
    /// </summary>
    public Script()
    {
        this.serializer = new JsonSerializer();
        this.serializer.NullValueHandling = NullValueHandling.Ignore;
    }

    /// <summary>
    /// Executes the action specified in the request and returns the result
    /// </summary>
    /// <returns></returns>
    public override async Task<HttpResponseMessage> ExecuteAsync()
    {
        var operationId = this.Context.OperationId;
        switch (operationId)
        {
            case "Match":
                {
                    return await this.HandleMatchRequestAsync().ConfigureAwait(false);
                }
            case "Test":
                {
                    return await this.HandleTestRequestAsync().ConfigureAwait(false);
                }
            case "Replace":
                {
                    return await this.HandleReplaceRequestAsync().ConfigureAwait(false);
                }
            case "Split":
                {
                    return await this.HandleSplitRequestAsync().ConfigureAwait(false);
                }
            default:
                {
                    return this.CreateBadRequestResponse();
                }

        }
    }

    /// <summary>
    /// Handles the /Replace action request
    /// </summary>
    /// <returns></returns>
    private async Task<HttpResponseMessage> HandleReplaceRequestAsync()
    {
        var contentAsString = await this.Context.Request.Content.ReadAsStringAsync().ConfigureAwait(false);
        var request = JsonConvert.DeserializeObject<RegexRequest>(contentAsString);

        // construct regex object now
        if (request == null)
        {
            return this.CreateBadRequestResponse();
        }

        var regex = new Regex(request.Pattern);
        return new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(regex.Replace(request.Text, request.Replace))
        };

    }

    /// <summary>
    /// Handles the /Split action request
    /// </summary>
    /// <returns></returns>
    private async Task<HttpResponseMessage> HandleSplitRequestAsync()
    {
        var contentAsString = await this.Context.Request.Content.ReadAsStringAsync().ConfigureAwait(false);
        var request = JsonConvert.DeserializeObject<RegexRequest>(contentAsString);

        // construct regex object now
        if (request == null)
        {
            return this.CreateBadRequestResponse();
        }

        var regex = new Regex(request.Pattern);

        return new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = CreateJsonContent(JArray.FromObject(regex.Split(request.Text)).ToString())
        };

    }

    /// <summary>
    /// Handles /Test action request
    /// </summary>
    /// <returns></returns>
    private async Task<HttpResponseMessage> HandleTestRequestAsync()
    {
        var contentAsString = await this.Context.Request.Content.ReadAsStringAsync().ConfigureAwait(false);
        var request = JsonConvert.DeserializeObject<RegexRequest>(contentAsString);

        // construct regex object now
        if (request == null)
        {
            return this.CreateBadRequestResponse();
        }

        var regex = new Regex(request.Pattern);
        var result = regex.IsMatch(request.Text);

        return new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(result.ToString().ToLower())
        };
    }

    /// <summary>
    /// Handles /Match action request
    /// </summary>
    /// <returns></returns>
    private async Task<HttpResponseMessage> HandleMatchRequestAsync()
    {
        var contentAsString = await this.Context.Request.Content.ReadAsStringAsync().ConfigureAwait(false);
        var request = JsonConvert.DeserializeObject<RegexRequest>(contentAsString);

        // construct regex object now
        if (request == null)
        {
            return this.CreateBadRequestResponse();
        }

        var regex = new Regex(request.Pattern);
        var responseObject = new MatchResponse();

        foreach (Match match in regex.Matches(request.Text))
        {
            var matchObject = new RegexMatch
            {
                Success = match.Success,
                Value = match.Value,
                Index = match.Index,
                Length = match.Length,
            };

            foreach (Group group in match.Groups)
            {
                matchObject.Groups.Add(new RegexGroup
                {
                    Success = group.Success,
                    Index = group.Index,
                    Length = group.Length,
                    Name = group.Name,
                    Value = group.Value,
                });
            }

            responseObject.Matches.Add(matchObject);
        }

        return new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = CreateJsonContent(JObject.FromObject(responseObject).ToString())
        };
    }


    /// <summary>
    /// Creates and returns an HTTP response that indicates 400 - Bad Request
    /// </summary>
    /// <param name="ex">The details of the exception to add to the response</param>
    /// <returns></returns>
    private HttpResponseMessage CreateBadRequestResponse(Exception? ex = default)
    {
        var response = new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = CreateJsonContent($"{{ 'message': '{ex?.Message}', 'stackTrace': '{ex?.StackTrace}' }}")
        };
        return response;
    }

    /// <summary>
    /// Represents a Regular Expression action request
    /// </summary>
    public class RegexRequest
    {
        public string? Text { get; set; }
        public string? Pattern { get; set; }
        public string? Replace { get; set; }
    }

    /// <summary>
    /// Represents a regular expression matched group
    /// </summary>
    public class RegexGroup
    {
        public string? Name { get; set; }
        public string? Value { get; set; }
        public int Index { get; set; }
        public int Length { get; set; }
        public bool Success { get; set; }
    }

    /// <summary>
    /// Represents a regular expression match
    /// </summary>
    public class RegexMatch
    {
        public bool Success { get; set; }
        public string? Value { get; set; }
        public int Index { get; set; }
        public int Length { get; set; }
        public List<RegexGroup> Groups { get; } = new List<RegexGroup>();
    }

    /// <summary>
    /// Represents the response for the regular expression match request
    /// </summary>
    public class MatchResponse
    {
        public List<RegexMatch> Matches { get; } = new List<RegexMatch>();
    }
}
