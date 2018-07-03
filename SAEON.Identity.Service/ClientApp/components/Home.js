var __extends = (this && this.__extends) || (function () {
    var extendStatics = Object.setPrototypeOf ||
        ({ __proto__: [] } instanceof Array && function (d, b) { d.__proto__ = b; }) ||
        function (d, b) { for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p]; };
    return function (d, b) {
        extendStatics(d, b);
        function __() { this.constructor = d; }
        d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
    };
})();
import * as React from 'react';
var Home = /** @class */ (function (_super) {
    __extends(Home, _super);
    function Home() {
        return _super !== null && _super.apply(this, arguments) || this;
    }
    Home.prototype.render = function () {
        return (React.createElement("div", { className: "row" },
            React.createElement("div", { className: "col-md-12" },
                React.createElement("p", null, "The South African Environmental Observation Network (SAEON) was established in 2002. SAEON is a research platform funded by the Department of Science and Technology (DST) and managed by the National Research Foundation (NRF). SAEON is mandated to establish and manage long-term environmental observatories; maintain reliable long-term environmental data sets; promote access to data for research and/or informed decision making; and contribute to capacity building."),
                React.createElement("p", null,
                    "The SAEON Identity Service provides an OpenID Connect single signon for all SAEON websites. The Identity Service publishes a",
                    React.createElement("a", { href: "~/.well-known/openid-configuration" }, " discovery document "),
                    "where you can find metadata and links to all the endpoints, key material, etc."))));
    };
    return Home;
}(React.Component));
export { Home };
//# sourceMappingURL=Home.js.map