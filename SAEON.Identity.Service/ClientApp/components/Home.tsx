import * as React from 'react';
import { RouteComponentProps } from 'react-router';

export class Home extends React.Component<RouteComponentProps<{}>, {}> {
    public render() {
        return (
            <div className="row">
                <div className="col-md-12">
                    <p>
                        The South African Environmental Observation Network (SAEON) was established in 2002. SAEON is a research platform funded by the
                        Department of Science and Technology (DST) and managed by the National Research Foundation (NRF).
                        SAEON is mandated to establish and manage long-term environmental observatories; maintain reliable long-term environmental data sets;
                        promote access to data for research and/or informed decision making; and contribute to capacity building.
                    </p>
                    <p>
                        The SAEON Identity Service provides an OpenID Connect single signon for all SAEON websites. The Identity Service publishes a
                        <a href="~/.well-known/openid-configuration"> discovery document </a>
                        where you can find metadata and links to all the endpoints, key material, etc.
                    </p>
                </div>
            </div>
        )
    }
}
