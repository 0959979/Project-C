#pragma checksum "/Users/yasemin/Documents/Inf_jaar_2/Project-C/Views/Doctor/SubmitDoctorAccount.cshtml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "2e8f6fd520721ced89bfc1345f395ec114494779"
// <auto-generated/>
#pragma warning disable 1591
[assembly: global::Microsoft.AspNetCore.Razor.Hosting.RazorCompiledItemAttribute(typeof(AspNetCore.Views_Doctor_SubmitDoctorAccount), @"mvc.1.0.view", @"/Views/Doctor/SubmitDoctorAccount.cshtml")]
[assembly:global::Microsoft.AspNetCore.Mvc.Razor.Compilation.RazorViewAttribute(@"/Views/Doctor/SubmitDoctorAccount.cshtml", typeof(AspNetCore.Views_Doctor_SubmitDoctorAccount))]
namespace AspNetCore
{
    #line hidden
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Rendering;
    using Microsoft.AspNetCore.Mvc.ViewFeatures;
#line 1 "/Users/yasemin/Documents/Inf_jaar_2/Project-C/Views/_ViewImports.cshtml"
using zorgapp;

#line default
#line hidden
#line 2 "/Users/yasemin/Documents/Inf_jaar_2/Project-C/Views/_ViewImports.cshtml"
using zorgapp.Models;

#line default
#line hidden
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"a146ad9f6c52ab1ed6d420dff8fc74b81e946f78", @"/Views/Doctor/SubmitDoctorAccount.cshtml")]
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"9fb9c266c6f46fd0d0d62b48ce0943fbf90530da", @"/Views/_ViewImports.cshtml")]
    public class Views_Doctor_SubmitDoctorAccount : global::Microsoft.AspNetCore.Mvc.Razor.RazorPage<zorgapp.Models.Doctor>
    {
        #pragma warning disable 1998
        public async override global::System.Threading.Tasks.Task ExecuteAsync()
        {
            BeginContext(30, 46, true);
            WriteLiteral("\r\n<h2>New doctor account has been created for ");
            EndContext();
            BeginContext(75, 21, false);
#line 3 "/Users/yasemin/Documents/Inf_jaar_2/Project-C/Views/Doctor/SubmitDoctorAccount.cshtml"
                                       Write(ViewData["FirstName"]);

#line default
#line hidden
            EndContext();
            BeginContext(98, 5, true);
            WriteLiteral("</h2>");
            EndContext();
        }
        #pragma warning restore 1998
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.ViewFeatures.IModelExpressionProvider ModelExpressionProvider { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.IUrlHelper Url { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.IViewComponentHelper Component { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.Rendering.IJsonHelper Json { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.Rendering.IHtmlHelper<zorgapp.Models.Doctor> Html { get; private set; }
    }
}
#pragma warning restore 1591
