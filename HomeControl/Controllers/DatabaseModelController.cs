using HomeControl.Database;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using System.Reflection;
using System.Threading.Tasks;

namespace HomeControl.Controllers
{
    [ApiController]
    [Route("Data/{ModelName}")]
    public class DatabaseModelController : Controller
    {
        public DatabaseModelController(IDatabaseConnectionService db)
        {
            _db = db;

            var dbType = _db.GetType();

            _selectSingleIdentityKey = dbType.GetMethod(nameof(IDatabaseConnectionService.SelectSingle), [typeof(int)]);
            _selectSingleStringKey = dbType.GetMethod(nameof(IDatabaseConnectionService.SelectSingle), [typeof(string)]);
            _select = dbType.GetMethod(nameof(IDatabaseConnectionService.Select));
            _insert = dbType.GetMethod(nameof(IDatabaseConnectionService.Insert));
            _update = dbType.GetMethod(nameof(IDatabaseConnectionService.Update));
            _delete = dbType.GetMethod(nameof(IDatabaseConnectionService.Delete));
        }

        private readonly IDatabaseConnectionService _db;

        private readonly MethodInfo _selectSingleIdentityKey;
        private readonly MethodInfo _selectSingleStringKey;
        private readonly MethodInfo _select;
        private readonly MethodInfo _insert;
        private readonly MethodInfo _update;
        private readonly MethodInfo _delete;

        [HttpPost]
        public async Task<IActionResult> OnPost([FromRoute] string modelName)
        {
            if (!_db.TryGetMetadata(modelName, out var modelType, out _)) return NotFound();

            object model;

            try
            {
                model = await System.Text.Json.JsonSerializer.DeserializeAsync(Request.Body, modelType);
            }
            catch
            {
                return BadRequest();
            }

            var genericInsertMethod = _insert.MakeGenericMethod(modelType);

            try
            {
                var query = (IQuery)genericInsertMethod.Invoke(_db, [model]);

                await query.ExecuteAsync();
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            return Json(model);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> OnGet([FromRoute] string modelName, [FromRoute] string id)
        {
            if (id == null) return NotFound();

            if (!_db.TryGetMetadata(modelName, out var modelType, out var metadata)) return NotFound();

            try
            {
                object resultModel = await SelectSingle(id, modelType, metadata);

                return Json(resultModel);
            }
            catch (BadHttpRequestException ex)
            {
                return BadRequest(ex.Message);
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpGet]
        public async Task<IActionResult> OnGet([FromRoute] string modelName)
        {
            if (!_db.TryGetMetadata(modelName, out var modelType, out var metadata)) return NotFound();

            var genericSelect = _select.MakeGenericMethod(modelType);

            var selectQuery = (ISelectQuery)genericSelect.Invoke(_db, []);

            if (Request.Query.Count > 0)
            {
                var where = selectQuery.Where();

                var queryfields = EnumerateQueryFields(Request.Query, metadata);

                var (lastQueryParameter, _) = queryfields.LastOrDefault();

                foreach (var (queryParameter, field) in queryfields)
                {
                    IStatement whereStatement;

                    switch (queryParameter.Key[field.Name.Length..].ToLowerInvariant())
                    {
                        case "":
                            whereStatement = where.Compare(field, ComparisonOperator.Equals, Convert.ChangeType(queryParameter.Value.FirstOrDefault(), field.PropertyInfo.PropertyType));
                            break;
                        case "_greater":
                            whereStatement = where.Compare(field, ComparisonOperator.GreaterThan, Convert.ChangeType(queryParameter.Value.FirstOrDefault(), field.PropertyInfo.PropertyType));
                            break;
                        case "_smaller":
                            whereStatement = where.Compare(field, ComparisonOperator.SmallerThan, Convert.ChangeType(queryParameter.Value.FirstOrDefault(), field.PropertyInfo.PropertyType));
                            break;
                        default: return BadRequest();
                    }

                    if (!queryParameter.Equals(lastQueryParameter)) where = whereStatement.And();
                }
            }

            var result = await GetAsyncMethodResult(await selectQuery.ExecuteAsync());

            return Json(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> OnDelete([FromRoute] string modelName, [FromRoute] string id)
        {
            if (id == null) return NotFound();

            if (!_db.TryGetMetadata(modelName, out var modelType, out var metadata)) return NotFound();

            try
            {
                var model = await SelectSingle(id, modelType, metadata);

                var genericDelete = _delete.MakeGenericMethod(modelType);

                await (Task)genericDelete.Invoke(_db, [model]);

                return Ok();
            }
            catch (BadHttpRequestException ex)
            {
                return BadRequest(ex.Message);
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        private async Task<object> SelectSingle(string id, Type modelType, DatabaseModelMetadata metadata)
        {
            var primaryKeyField = metadata.Fields.OfType<PrimaryKeyField>().FirstOrDefault() ?? throw new Exception("Invalid Model Metadata.");

            object primaryKey;

            MethodInfo genericSelectSingle;

            if (primaryKeyField.IsIdentity)
            {
                if (!int.TryParse(id, out var intId)) throw new BadHttpRequestException("Unable to parse Id from Route.");
                primaryKey = intId;

                genericSelectSingle = _selectSingleIdentityKey.MakeGenericMethod(modelType);
            }
            else
            {
                primaryKey = id;
                genericSelectSingle = _selectSingleStringKey.MakeGenericMethod(modelType);
            }

            var query = (IResultQuery)genericSelectSingle.Invoke(_db, [primaryKey]);

            return await GetAsyncMethodResult(await query.ExecuteAsync());
        }

        private static async Task<object> GetAsyncMethodResult(object methodResult)
        {
            if (methodResult is Task taskResult)
            {
                await taskResult;

                methodResult = methodResult.GetType().GetProperty(nameof(Task<DatabaseModel>.Result)).GetValue(methodResult, null);
            }

            return methodResult;
        }

        private static IEnumerable<(KeyValuePair<string, StringValues>, DatabaseColumnField)> EnumerateQueryFields(IQueryCollection query, DatabaseModelMetadata metadata)
        {
            foreach (var queryParameter in query)
            {
                var field = metadata.Fields.OfType<DatabaseColumnField>().FirstOrDefault(field => field.Name.StartsWith(queryParameter.Key, StringComparison.OrdinalIgnoreCase));

                if (field == null) continue;

                yield return (queryParameter, field);
            }
        }
    }
}