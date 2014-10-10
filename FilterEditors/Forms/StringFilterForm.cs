using System;
using System.Linq;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.DisplayManagement;
using Orchard.Forms.Services;
using Orchard.Localization;

namespace MainBit.Projections.SimilarContent.FilterEditors.Forms {

    public class StringFilterForm : IFormProvider {
        public const string FormName = "StringFilterSimilarContent";

        protected dynamic Shape { get; set; }
        public Localizer T { get; set; }

        public StringFilterForm(IShapeFactory shapeFactory) {
            Shape = shapeFactory;
            T = NullLocalizer.Instance;
        }

        public void Describe(DescribeContext context) {
            Func<IShapeFactory, object> form =
                shape => {

                    var f = Shape.Form(
                        Id: "StringFilter",
                        _Operator: Shape.SelectList(
                            Id: "operator", Name: "Operator",
                            Title: T("Operator"),
                            Size: 1,
                            Multiple: false
                        ),
                        _Value: Shape.TextBox(
                            Id: "value", Name: "Value",
                            Title: T("Value"),
                            Classes: new[] { "text medium", "tokenized" },
                            Description: T("Enter the value the string should be. Word separator is ';'")
                            ),
                        _OperatorUndefined: Shape.SelectList(
                            Id: "operatorUndefined", Name: "OperatorUndefined",
                            Title: T("Filter by value if comparision value undefined"),
                            Size: 1,
                            Multiple: false
                            )
                        );

                    f._Operator.Add(new SelectListItem { Value = Convert.ToString(StringOperator.Equals), Text = T("Is equal to").Text });
                    f._Operator.Add(new SelectListItem { Value = Convert.ToString(StringOperator.NotEquals), Text = T("Is not equal to").Text });
                    f._Operator.Add(new SelectListItem { Value = Convert.ToString(StringOperator.Contains), Text = T("Contains").Text });
                    f._Operator.Add(new SelectListItem { Value = Convert.ToString(StringOperator.ContainsAny), Text = T("Contains any word").Text });
                    f._Operator.Add(new SelectListItem { Value = Convert.ToString(StringOperator.ContainsAll), Text = T("Contains all words").Text });
                    f._Operator.Add(new SelectListItem { Value = Convert.ToString(StringOperator.Starts), Text = T("Starts with").Text });
                    f._Operator.Add(new SelectListItem { Value = Convert.ToString(StringOperator.NotStarts), Text = T("Does not start with").Text });
                    f._Operator.Add(new SelectListItem { Value = Convert.ToString(StringOperator.Ends), Text = T("Ends with").Text });
                    f._Operator.Add(new SelectListItem { Value = Convert.ToString(StringOperator.NotEnds), Text = T("Does not end with").Text });
                    f._Operator.Add(new SelectListItem { Value = Convert.ToString(StringOperator.NotContains), Text = T("Does not contain").Text });

                    f._OperatorUndefined.Add(new SelectListItem { Value = Convert.ToString(StringUndefinedOperator.Any), Text = T("Any").Text });
                    f._OperatorUndefined.Add(new SelectListItem { Value = Convert.ToString(StringUndefinedOperator.Undefined), Text = T("Undefined").Text });

                    return f;
                };

            context.Form(FormName, form);

        }

        public static Action<IHqlExpressionFactory> GetFilterPredicate(dynamic formState, string property) {
            var op = (StringOperator)Enum.Parse(typeof(StringOperator), Convert.ToString(formState.Operator));
            object value = Convert.ToString(formState.Value);
            var opUndef = (StringUndefinedOperator)Enum.Parse(typeof(StringUndefinedOperator), Convert.ToString(formState.OperatorUndefined));

            if (string.IsNullOrEmpty((string)value))
            {
                switch (opUndef)
                {
                    case StringUndefinedOperator.Any:
                        throw new NotNeedApplyFilterException(); //return null; //return x => x.Or(x1 => x1.IsNotNull(property), x2 => x2.IsNull(property));
                    case StringUndefinedOperator.Undefined:
                        return x => x.IsEmpty(property);
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            switch (op) {
                case StringOperator.Equals:
                    return x => x.Eq(property, value);
                case StringOperator.NotEquals:
                    return x => x.Not(y => y.Eq(property, value));
                case StringOperator.Contains:
                    return x => x.Like(property, Convert.ToString(value), HqlMatchMode.Anywhere);
                case StringOperator.ContainsAny:
                    var values1 = Convert.ToString(value).Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()).ToArray();
                    var predicates1 = values1.Skip(1).Select<string, Action<IHqlExpressionFactory>>(x => y => y.Like(property, x, HqlMatchMode.Anywhere)).ToArray();
                    return x => x.Disjunction(y => y.Like(property, values1[0], HqlMatchMode.Anywhere), predicates1);
                case StringOperator.ContainsAll:
                    var values2 = Convert.ToString(value).Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()).ToArray();
                    var predicates2 = values2.Skip(1).Select<string, Action<IHqlExpressionFactory>>(x => y => y.Like(property, x, HqlMatchMode.Anywhere)).ToArray();
                    return x => x.Conjunction(y => y.Like(property, values2[0], HqlMatchMode.Anywhere), predicates2);
                case StringOperator.Starts:
                    return x => x.Like(property, Convert.ToString(value), HqlMatchMode.Start);
                case StringOperator.NotStarts:
                    return y => y.Not(x => x.Like(property, Convert.ToString(value), HqlMatchMode.Start));
                case StringOperator.Ends:
                    return x => x.Like(property, Convert.ToString(value), HqlMatchMode.End);
                case StringOperator.NotEnds:
                    return y => y.Not(x => x.Like(property, Convert.ToString(value), HqlMatchMode.End));
                case StringOperator.NotContains:
                    return y => y.Not(x => x.Like(property, Convert.ToString(value), HqlMatchMode.Anywhere));
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static LocalizedString DisplayFilter(string fieldName, dynamic formState, Localizer T) {
            var op = (StringOperator)Enum.Parse(typeof(StringOperator), Convert.ToString(formState.Operator));
            string value = Convert.ToString(formState.Value);
            var opUndef = (BooleanUndefinedOperator)Enum.Parse(typeof(BooleanUndefinedOperator), Convert.ToString(formState.OperatorUndefined));

            switch (op) {
                case StringOperator.Equals:
                    return T("{0} is equal to '{1}'", fieldName, value);
                case StringOperator.NotEquals:
                    return T("{0} is not equal to '{1}'", fieldName, value);
                case StringOperator.Contains:
                    return T("{0} contains '{1}'", fieldName, value);
                case StringOperator.ContainsAny:
                    return T("{0} contains any of '{1}'", fieldName, new LocalizedString(String.Join("', '", value.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))));
                case StringOperator.ContainsAll:
                    return T("{0} contains all '{1}'", fieldName, new LocalizedString(String.Join("', '", value.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))));
                case StringOperator.Starts:
                    return T("{0} starts with '{1}'", fieldName, value);
                case StringOperator.NotStarts:
                    return T("{0} does not start with '{1}'", fieldName, value);
                case StringOperator.Ends:
                    return T("{0} ends with '{1}'", fieldName, value);
                case StringOperator.NotEnds:
                    return T("{0} does not end with '{1}'", fieldName, value);
                case StringOperator.NotContains:
                    return T("{0} does not contain '{1}'", fieldName, value);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    public enum StringOperator {
        Equals,
        NotEquals,
        Contains,
        ContainsAny,
        ContainsAll,
        Starts,
        NotStarts,
        Ends,
        NotEnds,
        NotContains,
    }

    public enum StringUndefinedOperator
    {
        Any,
        Undefined
    }
}